using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GeniView.Cloud.Repository
{
    public class BatteriesDataRepository : IDisposable
    {
        public DBHelper _dBHelp = new DBHelper();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly GeniViewCloudDataRepository _db;

        public BatteriesDataRepository(GeniViewCloudDataRepository db)
        {
            _db = db;
        }


        #region Batteries

        public Battery Create(Battery battery, GeniViewCloudDataRepository db)
        {

            if (battery != null)
            {
                db.Batteries.Add(battery);

                db.SaveChanges();
            }

            return battery;
        }

        public List<Battery> CreateBatch(List<Battery> batteries, GeniViewCloudDataRepository db)
        {
            List<long> snCode = batteries.Select(x => x.SerialNumberCode.GetValueOrDefault()).Where(y => y != null).ToList();
            var exist = FindBySNCode(snCode, db).ToList();

            var remove = batteries.RemoveAll(x => exist.Any(e => e.SerialNumberCode == x.SerialNumberCode) == true);

            if (batteries != null && batteries.Count >= 1)
            {
                _dBHelp.BatchInsert(db, db.Batteries, batteries);
            }

            if (batteries.Any() == true)
            {
                var log = batteries.Select(x => new { x.SerialNumber, x.SerialNumberCode });

                _logger.Debug($"Battery CreateBatch {batteries.Count}  Remove {remove} log {JsonConvert.SerializeObject(log)}");
            }

            return batteries;
        }

        public List<Battery> CreateBatch(Dictionary<string, Battery> batteries, GeniViewCloudDataRepository db)
        {
            List<Battery> result = new List<Battery>();

            foreach (var item in batteries)
            {
                //var battery = item.Value;
                var battery = new Battery(item.Value);

                result.Add(battery);

            }

            result = CreateBatch(result, db);

            return result;
        }

        public List<Battery> UpdateBatch(List<Battery> batteries, GeniViewCloudDataRepository db)
        {
            StringBuilder sb = new StringBuilder();

            List<long> snCode = batteries.Select(x => x.SerialNumberCode.GetValueOrDefault()).Where(y => y != null).ToList();
            var exist = FindBySNCode(snCode, db).ToList();

            //Remove exist
            var ret = batteries.Where(x => exist.Any(e => x.SerialNumberCode == e.SerialNumberCode) == false);

            var remove = batteries.RemoveAll(x => exist.Any(e => x.SerialNumberCode == e.SerialNumberCode) == false);

            if (batteries != null && batteries.Count >= 1)
            {
                foreach (var battery in batteries)
                {
                    var orgion = exist.Where(x => x.SerialNumberCode == battery.SerialNumberCode).FirstOrDefault();

                    if (orgion != null)
                    {
                        battery.ID = orgion.ID;
                    }
                }

                _dBHelp.UpdateAll(db, db.Batteries, batteries);
            }

            if (batteries.Any() == true)
            {
                var log = batteries.Select(x => new { x.SerialNumber, x.SerialNumberCode });
                _logger.Info($"Battery UpdateBatch {batteries.Count}  Remove {remove} log {JsonConvert.SerializeObject(log)}");
            }

            return batteries;
        }

        public List<Battery> UpdateBatch(Dictionary<string, Battery> batteries, GeniViewCloudDataRepository db)
        {
            List<Battery> result = new List<Battery>();

            foreach (var item in batteries)
            {
                //var battery = item.Value;
                var battery = new Battery(item.Value);

                result.Add(battery);

            }

            result = UpdateBatch(result, db);

            return result;
        }

        public List<Battery> CreateAndUpdateBatch(Dictionary<string, Battery> batteries, GeniViewCloudDataRepository db)
        {
            List<Battery> result = new List<Battery>();
            var update = UpdateBatch(batteries, db);
            var create = CreateBatch(batteries, db);

            result.AddRange(update);
            result.AddRange(create);

            return result;
        }

        public IEnumerable<Battery> GetBatteries(GeniViewCloudDataRepository db)
        {
            var ret = db.Batteries;

            return ret;
        }

        public IEnumerable<BatteriesListViewModel> GetBatteries(long? communityID, long? groupID, bool includeAllSubGroups, string customFilter = null)
        {
            var normal = new ExtraInfo { Name = "Normal", Color = GlobalSettings.SuccessColor };
            var alert = new ExtraInfo { Name = "Alert", Color = GlobalSettings.AlertColor };

            var db = _db;
            {

                // Query doesn't need to track changes and detect changes, because query is readonly(return list)

                var mainQuery = BuildOptimizedQuery(communityID, db);

                // Get All Group and it's childred 
                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    var groupdb = new GroupsDataRepository(_db);
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    mainQuery = (from m in mainQuery.Where(x => x.Group != null)
                                 join ch in allChildrenGroups on m.Group.ID equals ch.ID
                                 select m).AsQueryable();

                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    mainQuery = mainQuery.Where(x => x.Group.ID == groupID);
                }

                // Optional Filter
                if (customFilter != null)
                {
                    switch (customFilter)
                    {
                        case "Charging":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentBatteryLog
                                        where row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes && row.Status == BatteryStates.Charging
                                        select m;
                            break;
                        case "Discharging":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentBatteryLog
                                        where row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes && row.Status == BatteryStates.PoweringSystem
                                        select m;
                            break;
                        case "IdleNeedsCharging":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentBatteryLog
                                        where row != null && row.Timestamp >= GlobalSettings.OfflineRangeInDays
                                                          && (row.Timestamp < GlobalSettings.OnlineRangeInMinutes
                                                          || (row.Status != BatteryStates.Charging && row.Status != BatteryStates.PoweringSystem && row.SlowChangingDataA.RelativeStateOfCharge < GlobalSettings.IsStateOfChargeReadyToUse))
                                        select m;
                            break;
                        case "IdleReadytoUse":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentBatteryLog
                                        where row != null && row.Timestamp >= GlobalSettings.OfflineRangeInDays
                                                          && (row.Timestamp < GlobalSettings.OnlineRangeInMinutes
                                                          || (row.Status != BatteryStates.Charging && row.Status != BatteryStates.PoweringSystem && row.SlowChangingDataA.RelativeStateOfCharge >= GlobalSettings.IsStateOfChargeReadyToUse))
                                        select m;
                            break;
                        case "Unknown":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentBatteryLog
                                        where row != null && row.Timestamp <= GlobalSettings.OfflineRangeInDays
                                        select m;
                            break;
                        default:
                            mainQuery = null;
                            break;
                    }
                }

                return mainQuery
                    .ToList()
                    .GroupBy(x => x.ID)
                    .Select(g => g.First())
                    .ToList();
            }
        }

        public BatteryDetailViewModel GetBatteryDetails(long serialNumber)
        {
            var db = _db;
            {
                // Load battery + collections from SQL (only the WHERE filter runs in SQL).
                // All arithmetic is done in C# below to avoid SQL Server divide-by-zero
                // errors caused by the query optimizer evaluating all CASE branches.
                var b = db.Batteries
                    .Include(x => x.AgentBatteryLogCollection)
                    .Include(x => x.BatterySettingsCollection)
                    .Where(x => x.SerialNumberCode == serialNumber && x.IsDeactivated == false)
                    .FirstOrDefault();

                if (b == null) return null;

                // All of the following runs in C# — no SQL divide-by-zero possible.
                var row      = b.AgentBatteryLogCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault();
                var settings = b.BatterySettingsCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault();
                var logs     = b.AgentBatteryLogCollection;

                // Note : Voltage Calculation max = 25.2 and min = 18;
                //      : Used Formula like (dataVoltage - 18 ) * 100 / (25.2 - 18), because we can calculate when min = 0, not min = 18
                return new BatteryDetailViewModel
                {
                    Battery            = b,
                    LastSettings       = settings,
                    LastAgentBatteryLog = row,
                    FirstSeenOn        = logs.Any() ? logs.Min(t => t.Timestamp) : (DateTime?)null,
                    LastSeenOn         = logs.Any() ? logs.Max(t => t.Timestamp) : (DateTime?)null,

                    Voltage = row != null
                        ? new ExtraInfo { Name = Math.Round(((row.OperatingData.Voltage - 18) * 100) / (25.2 - 18), 2).ToString(), Color = GlobalSettings.SuccessColor, Value = Math.Round(Math.Abs(row.OperatingData.Voltage), 2), Title = "" }
                        : new ExtraInfo { Name = "0", Color = GlobalSettings.AlertColor, Value = 0, Title = "" },

                    Charging = (row != null && row.Status == BatteryStates.Charging)
                        ? new ExtraInfo { Name = Math.Abs(Math.Round((row.OperatingData.Current / 4) * 100)).ToString(), Color = GlobalSettings.SuccessColor, Value = Math.Round(Math.Abs(row.OperatingData.Current), 2), Title = "Charge Current" }
                        : (row != null && row.Status == BatteryStates.PoweringSystem)
                            ? new ExtraInfo { Name = Math.Abs(Math.Round((row.OperatingData.Current / 9.2) * 100)).ToString(), Color = GlobalSettings.SuccessColor, Value = Math.Round(Math.Abs(row.OperatingData.Current), 2), Title = "Discharge Current" }
                            : new ExtraInfo { Name = "0", Color = GlobalSettings.SuccessColor, Value = row != null ? Math.Round(Math.Abs(row.OperatingData.Current), 1) : 0, Title = "Idle Current" },

                    RemainingCapacity = row != null
                        ? new ExtraInfo { Name = row.SlowChangingDataA.RelativeStateOfCharge.ToString(), Color = GlobalSettings.SuccessColor, Value = row.SlowChangingDataA.RemainingCapacity, Title = "" }
                        : new ExtraInfo { Name = "0", Color = GlobalSettings.AlertColor, Value = 0, Title = "" },

                    CalcCapacity = row != null
                        ? (b.DesignCapacity > 0 && row.SlowChangingDataA.CalculatedCapacity < b.DesignCapacity)
                            ? new ExtraInfo { Name = Math.Round(row.SlowChangingDataA.CalculatedCapacity / b.DesignCapacity * 100, 0).ToString(), Color = GlobalSettings.SuccessColor, Value = row.SlowChangingDataA.CalculatedCapacity, Title = "" }
                            : new ExtraInfo { Name = "100", Color = GlobalSettings.SuccessColor, Value = row.SlowChangingDataA.CalculatedCapacity, Title = "" }
                        : new ExtraInfo { Name = "0", Color = GlobalSettings.AlertColor, Value = 100, Title = "" },

                    State = row != null ? row.Status : BatteryStates.Unknown,
                };
            }
        }

        public Battery FindBatteryByID(long id, long? communityID = null, long? groupID = null)
        {
            var db = _db;
            {

                var mainQuery = db.Batteries
                                  .Include(x => x.Community)
                                  .Include(x => x.Group)
                                  .Where(x => (communityID == null ? true : x.Community.ID == communityID)
                                           && (groupID == null ? true : x.Group.ID == groupID)
                                           && (x.ID == id)
                                           && x.IsDeactivated == false
                                         )
                                .FirstOrDefault();
                return mainQuery;
            }
        }

        /// <summary>
        /// Returns the most recent AgentBatteryLog timestamp for a battery (UTC).
        /// Used to default the Graphs page date range to the battery's last-seen time.
        /// </summary>
        public DateTime? GetLastSeen(long batteryID)
        {
            var db = _db;
            {
                return db.AgentBatteryLog
                         .Where(l => l.Battery_ID == batteryID)
                         .Max(l => (DateTime?)l.Timestamp);
            }
        }

        public IEnumerable<Battery> FindBySN(List<string> serialNumber, GeniViewCloudDataRepository db)
        {

            var result = db.Batteries.Include(x => x.Community)
                                        .Include(x => x.Group)
                                        .Where(x => x.IsDeactivated == false && serialNumber.Any(s => s == x.SerialNumber));
            return result;
        }

        public IEnumerable<Battery> FindBySNCode(List<long> serialNumberCode, GeniViewCloudDataRepository db)
        {

            var result = db.Batteries.Include(x => x.Community)
                                        .Include(x => x.Group)
                                        .Where(x => x.IsDeactivated == false && serialNumberCode.Any(s => s == x.SerialNumberCode));
            return result;
        }

        public IEnumerable<Battery> FindBySNCode(List<Battery> batteries, GeniViewCloudDataRepository db)
        {
            var snCode = batteries
                .Where(x => x.SerialNumberCode.HasValue == true)
                .Select(y => y.SerialNumberCode.GetValueOrDefault()).ToList();

            var result = FindBySNCode(snCode, db);

            return result;
        }

        public void UpdateBattery(BatteriesListViewModel battery, long? groupID)
        {
            var db = _db;
            {
                var model = db.Batteries.Include(x => x.Group).AsEnumerable();
                var originalbattery = model.Where(x => x.ID == battery.ID && x.IsDeactivated == false).FirstOrDefault();
                if (groupID != null)
                {
                    originalbattery.Group = db.Groups.Find(groupID);
                }
                else
                {
                    originalbattery.Group = null;
                }

                db.Entry(originalbattery).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public IEnumerable<BatteryModel> GetBatteryChartModel(long batteryID, DateTime beginDate, DateTime endDate, int pointCount = 500)
        {
            var db = _db;
            {
                db.Database.SetCommandTimeout(300);
                // Query doesn't need to track changes and detect changes, because query is readonly(return list)
                ApplicationUser currentUser = null;
                // Convert from Locat to UTC
                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(beginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(endDate, currentUser);

                var originalBattery = db.Batteries.FirstOrDefault(x => x.ID == batteryID);

                if (originalBattery != null)
                {
                    int index = 1;
                    var query = db.Entry(originalBattery)
                                       .Collection(x => x.AgentBatteryLogCollection)
                                       .Query()
                                       .Where(x => x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate && x.Battery.IsDeactivated == false)
                                       .OrderBy(x => x.Timestamp)
                                       .AsNoTracking()
                                       .AsEnumerable()
                                       .Select(x => new BatteryModel()
                                       {
                                           LogIndex = index++,
                                           Voltage = x.OperatingData.Voltage,
                                           AverageCurrent = x.OperatingData.AverageCurrent,
                                           RelativeStateofCharge = x.SlowChangingDataA.RelativeStateOfCharge,
                                           Current = x.OperatingData.Current,
                                           RemainingCapacity = x.SlowChangingDataA.RemainingCapacity,
                                           Temperature = x.SlowChangingDataB.BatteryInternalTemperature,
                                           Power = Math.Round(x.OperatingData.Voltage * x.OperatingData.Current, 2),
                                           // Added by July Snag 2016
                                           CalculatedCapacity = x.SlowChangingDataA.CalculatedCapacity,
                                           CycleCount = x.OperatingData.CycleCount,
                                           TimeStampt = x.Timestamp
                                       }).ToList();

                    int count = query.Count();
                    count = count < pointCount ? 1 : count / pointCount;

                    var model = query.GroupBy(g => new { idx = g.LogIndex / count })
                                     .Select(s => new BatteryModel()
                                     {
                                         LogIndex = s.Key.idx,
                                         Voltage = s.Average(x => x.Voltage),
                                         AverageCurrent = s.Average(x => x.AverageCurrent),
                                         RelativeStateofCharge = (int)s.Average(x => x.RelativeStateofCharge),
                                         Current = s.Average(x => x.Current),
                                         RemainingCapacity = s.Average(x => x.RemainingCapacity),
                                         Temperature = (int)s.Average(x => x.Temperature),
                                         Power = s.Average(x => x.Power),
                                         CalculatedCapacity = s.Average(x => x.CalculatedCapacity),
                                         CycleCount = (int)s.Average(x => x.CycleCount),
                                         TimeStampt = s.Max(x => x.TimeStampt)
                                     }).ToList();

                    model.ForEach(x => x.LogDate = TimeZoneHelper.GetLocalDateTime(x.TimeStampt, currentUser));

                    return model;
                }

                return null;
            }
        }

        public IEnumerable<BatteryModel> GetBatteryChartModelByLog(long batteryID, DateTime beginDate, DateTime endDate, int pointCount = 500)
        {
            var db = _db;
            {
                db.Database.SetCommandTimeout(300);
                // Query doesn't need to track changes and detect changes, because query is readonly(return list)

                ApplicationUser currentUser = null;
                // Convert from Locat to UTC
                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(beginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(endDate, currentUser);

                var originalBattery = db.Batteries.FirstOrDefault(x => x.ID == batteryID && x.IsDeactivated == false);

                if (originalBattery != null)
                {
                    int index = 1;
                    var query = db.Entry(originalBattery)
                                       .Collection(x => x.InternalBatteryLogCollection)
                                       .Query()
                                       .Where(x => x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate)
                                       .OrderBy(x => x.Timestamp)
                                       .AsNoTracking()
                                       .AsEnumerable()
                                       .Select(x => new BatteryModel()
                                       {
                                           LogIndex = x.LogIndex,
                                           Voltage = x.Voltage,
                                           AverageCurrent = x.AverageCurrent,
                                           RelativeStateofCharge = x.RelativeStateOfCharge,
                                           Current = x.Current,
                                           RemainingCapacity = x.RemainingCapacity,
                                           Temperature = x.Temperature,
                                           Power = Math.Round(x.Voltage * x.Current, 2),
                                           TimeStampt = x.Timestamp
                                       }).ToList();

                    int count = query.Count();
                    count = count < pointCount ? 1 : count / pointCount;

                    var model = query.GroupBy(g => new { idx = g.LogIndex / count })
                                    .Select(s => new BatteryModel()
                                    {
                                        LogIndex = s.Key.idx,
                                        Voltage = s.Average(x => x.Voltage),
                                        AverageCurrent = s.Average(x => x.AverageCurrent),
                                        RelativeStateofCharge = (int)s.Average(x => x.RelativeStateofCharge),
                                        Current = s.Average(x => x.Current),
                                        RemainingCapacity = s.Average(x => x.RemainingCapacity),
                                        Temperature = (int)s.Average(x => x.Temperature),
                                        Power = s.Average(x => x.Power),
                                        TimeStampt = s.Max(x => x.TimeStampt)
                                    }).ToList();
                    model.ForEach(x => x.LogDate = TimeZoneHelper.GetLocalDateTime(x.TimeStampt, currentUser));

                    return model;
                }
                return null;
            }
        }

        public List<InternalBatteryLog> GetBatteryHistoryLog(long id, DateTime beginDate, DateTime endDate, ApplicationUser currentUser, bool isPeriodicDataTriggerIncluded, int count = 100)
        {
            var db = _db;
            {
                List<InternalBatteryLog> model = new List<InternalBatteryLog>();

                // Convert from Locat to UTC 
                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(beginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(endDate, currentUser);

                var originalDevice = db.Batteries.FirstOrDefault(x => x.ID == id && x.IsDeactivated == false);
                if (originalDevice != null)
                {
                    int index = 1;
                    model = db.Entry(originalDevice)
                              .Collection(x => x.InternalBatteryLogCollection)
                              .Query()
                              .Include(x => x.Agent)
                              .Where(x => x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate && (isPeriodicDataTriggerIncluded ? true : x.EventCodeRaw != 50))
                              .OrderByDescending(x => x.Timestamp) // Order by TimeStamp July Snag 2016
                              .AsNoTracking()
                              .Select(x => x).Take(count).ToList();
                    return model;
                }
                return model;
            }
        }
        #endregion

        #region Assign and Remove Batteries
        public List<AssignRemoveModel> GetAssignedBatteries(long communityID)
        {
            List<AssignRemoveModel> model = new List<AssignRemoveModel>();

            var db = _db;
            {

                model = (from m in db.Batteries.Include(x => x.Community)
                                               .Where(x => x.Community.ID == communityID)
                                               .AsEnumerable()
                         select new AssignRemoveModel()
                         {
                             IsChecked = false,
                             SerialNumber = m.SerialNumber,
                             BatteryBSN = m.SerialNumberCode,
                             CommunityName = m.Community.Name
                         }).ToList();
            }
            return model;
        }

        public void RemoveBatteries(AssignRemoveListModel model)
        {
            var setList = new List<AssignRemoveModel>();
            if (model.DeviceList != null)
            {
                setList = model.DeviceList.Where(x => x.IsChecked == true).ToList();
            }
            var db = _db;
            {
                foreach (var item in setList)
                {
                    var battery = db.Batteries.Include(x => x.Community)
                                              .Where(x => x.SerialNumberCode == item.BatteryBSN)
                                              .FirstOrDefault();
                    battery.Community = null;
                    db.Entry(battery).State = EntityState.Modified;
                }
                db.SaveChanges();
            }

        }

        public List<AssignRemoveModel> GetUnassignedBatteries()
        {
            List<AssignRemoveModel> model = new List<AssignRemoveModel>();

            var db = _db;
            {

                model = (from m in db.Batteries.Include(x => x.Community)
                                               .Where(x => x.Community.ID == null)
                                               .AsEnumerable()
                         select new AssignRemoveModel()
                         {
                             IsChecked = false,
                             BatteryBSN = m.SerialNumberCode,
                             SerialNumber = m.SerialNumber,
                         }).ToList();
            }
            return model;
        }

        public void AssignBatteries(AssignRemoveListModel model)
        {
            var setList = new List<AssignRemoveModel>();
            if (model.DeviceList != null)
            {
                setList = model.DeviceList.Where(x => x.IsChecked == true).ToList();
            }
            var db = _db;
            {

                foreach (var item in setList)
                {
                    var battery = db.Batteries.Include(x => x.Community)
                                              .Where(x => x.SerialNumberCode == item.BatteryBSN)
                                              .FirstOrDefault();
                    battery.Community = db.Communities.Find(model.CommunityID);
                    db.Entry(battery).State = EntityState.Modified;
                }
                db.SaveChanges();
            }

        }
        #endregion

        #region Active / Deactivated Batteries
        public List<ActivateDeactivatedModel> GetDeactivatedBatteries()
        {
            List<ActivateDeactivatedModel> model = new List<ActivateDeactivatedModel>();
            var db = _db;
            {

                model = (from m in db.Batteries
                                     .Include(x => x.Community)
                                     .Include(x => x.Group)
                                     .Where(x => x.IsDeactivated == true)
                                     .AsEnumerable()
                         select new ActivateDeactivatedModel()
                         {
                             IsChecked = false,
                             SerialNumber = m.SerialNumber,
                             BatteryBSN = m.SerialNumberCode,
                             CommunityName = m.Community != null ? m.Community.Name : "",
                             GroupName = m.Group != null ? m.Group.Name : ""
                         }).ToList();
            }
            return model;
        }

        public List<ActivateDeactivatedModel> GetActiveBatteries()
        {
            List<ActivateDeactivatedModel> model = new List<ActivateDeactivatedModel>();
            var db = _db;
            {

                model = (from m in db.Batteries
                                     .Include(x => x.Community)
                                     .Include(x => x.Group)
                                     .Where(x => x.IsDeactivated == false)
                                     .AsEnumerable()
                         select new ActivateDeactivatedModel()
                         {
                             IsChecked = false,
                             SerialNumber = m.SerialNumber,
                             BatteryBSN = m.SerialNumberCode,
                             CommunityName = m.Community != null ? m.Community.Name : "",
                             GroupName = m.Group != null ? m.Group.Name : ""
                         }).ToList();
            }
            return model;
        }

        public void ActivateDeactivateBatteries(List<ActivateDeactivatedModel> model, bool isDeactivated)
        {
            var setList = new List<ActivateDeactivatedModel>();
            if (model != null)
            {
                setList = model.Where(x => x.IsChecked == true).ToList();
            }

            var db = _db;
            {

                foreach (var item in setList)
                {
                    var device = db.Batteries.Where(x => x.SerialNumberCode == item.BatteryBSN).FirstOrDefault();
                    device.IsDeactivated = isDeactivated;
                    db.Entry(device).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        public IQueryable<BatteriesListViewModel> BuildOptimizedQuery(long? communityID, GeniViewCloudDataRepository db)
        {
            var offlineThreshold = DateTime.UtcNow.AddMinutes(-6);

            // Performance-optimised path: call the PostgreSQL LATERAL function
            // get_batteries_list() instead of the previous double GROUP BY + self-join.
            // Returns exactly one row per battery (latest log via LATERAL LIMIT 1).
            var rows = db.Database
                .SqlQueryRaw<BatteryListRaw>(
                    "SELECT * FROM get_batteries_list({0})",
                    communityID.HasValue ? (object)communityID.Value : DBNull.Value)
                .ToList();

            var result = rows.Select(r =>
            {
                var community = r.community_id.HasValue
                    ? new Community { ID = r.community_id.Value, Name = r.community_name }
                    : null;

                var group = r.group_id.HasValue
                    ? new Group { ID = r.group_id.Value, Name = r.group_name }
                    : null;

                // Reconstruct a minimal AgentBatteryLog so all existing downstream logic
                // (custom filters, Status/ChargingLevel/Temperature/Alert calculations)
                // works without any changes.
                AgentBatteryLog lastLog = null;
                if (r.last_seen_on.HasValue)
                {
                    lastLog = new AgentBatteryLog
                    {
                        Timestamp        = r.last_seen_on.Value,
                        SlowChangingDataA = new BatterySlowChangingDataA
                        {
                            RelativeStateOfCharge = r.relative_soc
                        },
                        SlowChangingDataB = new BatterySlowChangingDataB
                        {
                            BatteryInternalTemperature = r.internal_temp
                        },
                        OperatingData = new BatteryOperatingData
                        {
                            Current = r.operating_current
                        }
                    };
                }

                var relativeStateOfCharge = lastLog?.SlowChangingDataA?.RelativeStateOfCharge ?? 0;
                var operatingCurrent      = lastLog?.OperatingData?.Current ?? 0;
                var internalTemperature   = lastLog?.SlowChangingDataB?.BatteryInternalTemperature ?? 0;

                var statusName = relativeStateOfCharge < 30
                    ? "Charge Now"
                    : operatingCurrent > 0
                        ? "Charging"
                        : operatingCurrent < 0
                            ? "Discharging"
                            : "Idle";

                var statusColor = relativeStateOfCharge < 30
                    ? GlobalSettings.WarningColor
                    : operatingCurrent > 0
                        ? GlobalSettings.SuccessColor
                        : operatingCurrent < 0
                            ? "#90EE90"
                            : GlobalSettings.SuccessColor;

                return new BatteriesListViewModel
                {
                    ID                  = r.battery_id,
                    Battery             = new Battery
                    {
                        ID               = r.battery_id,
                        SerialNumber     = r.serial_number,
                        SerialNumberCode = r.serial_number_code,
                        IsDeactivated    = false
                    },
                    Group               = group,
                    Community           = community,
                    FirstSeenOn         = r.first_seen_on,
                    LastSeenOn          = r.last_seen_on,
                    LastAgentBatteryLog = lastLog,

                    isOnline = (r.last_seen_on != null && r.last_seen_on >= offlineThreshold),

                    Status =
                        (r.last_seen_on != null && r.last_seen_on >= offlineThreshold && lastLog != null)
                            ? new ExtraInfo { Name = statusName, Color = statusColor }
                            : new ExtraInfo { Name = "Offline", Color = GlobalSettings.AlertColor },

                    ChargingLevel =
                        (relativeStateOfCharge > GlobalSettings.SuccessChargingLVL)
                            ? new ExtraInfo { Name = relativeStateOfCharge.ToString(), Color = GlobalSettings.SuccessColor } :
                        (relativeStateOfCharge > GlobalSettings.AlertChargingLVL)
                            ? new ExtraInfo { Name = relativeStateOfCharge.ToString(), Color = GlobalSettings.WarningColor }
                            : new ExtraInfo { Name = relativeStateOfCharge.ToString(), Color = GlobalSettings.AlertColor },

                    Temperature =
                        (internalTemperature < GlobalSettings.SuccessTemperature)
                            ? new ExtraInfo { Name = internalTemperature.ToString(), Color = GlobalSettings.SuccessColor } :
                        (internalTemperature > GlobalSettings.AlertTemperature)
                            ? new ExtraInfo { Name = internalTemperature.ToString(), Color = GlobalSettings.AlertColor }
                            : new ExtraInfo { Name = internalTemperature.ToString(), Color = GlobalSettings.WarningColor },

                    Alert =
                        (internalTemperature > GlobalSettings.AlertTemperature || relativeStateOfCharge < 5)
                            ? new ExtraInfo { Name = "Alert",  Color = GlobalSettings.AlertColor }
                            : new ExtraInfo { Name = "Normal", Color = GlobalSettings.SuccessColor }
                };
            }).AsQueryable();

            return result;
        }

        /// <summary>
        /// Flat DTO used to receive results from the get_batteries_list() PostgreSQL function.
        /// Property names match the function's RETURNS TABLE column names exactly (lowercase).
        /// </summary>
        private class BatteryListRaw
        {
            public long     battery_id         { get; set; }
            public string   serial_number      { get; set; }
            public long?    serial_number_code { get; set; }
            public long?    community_id       { get; set; }
            public string   community_name     { get; set; }
            public long?    group_id           { get; set; }
            public string   group_name         { get; set; }
            public DateTime? first_seen_on     { get; set; }
            public DateTime? last_seen_on      { get; set; }
            public int      relative_soc       { get; set; }
            public int      internal_temp      { get; set; }
            public double   operating_current  { get; set; }
        }

    }
}
