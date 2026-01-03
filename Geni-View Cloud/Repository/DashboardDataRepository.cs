using GeniView.Cloud.Models;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Globalization;
using GeniView.Data.Hardware.Event;
using GeniView.Cloud.Common;

namespace GeniView.Cloud.Repository
{
    public class DashboardDataRepository : IDisposable
    {
        private const string CycleStatusStoredProcedureName = "dbo.usp_GetLatestBatteryCycleCount";
        private const string StateOfChargeStoredProcedureName = "dbo.usp_GetLatestBatteryStateOfCharge";

        private sealed class CycleStatusRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public int OperatingData_CycleCount { get; set; }
        }

        private sealed class StateOfChargeRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public int SlowChangingDataA_RelativeStateOfCharge { get; set; }
        }

        private Random randomDouble = new Random();

        #region Dashboard

        public CycleStatusModel GetCycleStatus(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => (communityID == null ? true : b.Community.ID == communityID) && b.IsDeactivated == false)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        GroupID = (long?)b.Group.ID
                    })
                    .ToList();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    var includedGroupIds = allChildrenGroups.Select(g => g.ID).ToHashSet();
                    includedGroupIds.Add(groupID.Value);

                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID != null && includedGroupIds.Contains(x.GroupID.Value))
                        .ToList();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    batteryQuery = batteryQuery.Where(x => x.GroupID == groupID).ToList();
                }

                var allowedBatteryIds = batteryQuery.Select(x => x.BatteryID).ToHashSet();

                // Power Modules is the number of batteries in current filter.
                var powerModulesCount = allowedBatteryIds.Count;

                // SP already returns latest row per Battery_ID.
                var spRows = db.Database.SqlQuery<CycleStatusRow>("EXEC " + CycleStatusStoredProcedureName)
                    .ToList();

                var rows = spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList();

                var low = 0;
                var high = 0;
                var eol = 0;

                foreach (var row in rows)
                {
                    // Keep the existing thresholds but map them to the UX wording.
                    if (row.OperatingData_CycleCount < 10)
                    {
                        low++;
                    }
                    else if (row.OperatingData_CycleCount <= 20)
                    {
                        high++;
                    }
                    else
                    {
                        eol++;
                    }
                }

                var total = low + high + eol;

                var result = new CycleStatusModel
                {
                    LowCount = low,
                    HighCount = high,
                    EndOfLifeCount = eol,
                    TotalCount = total,
                    PowerModulesCount = powerModulesCount,
                    AverageCycleCount = (int)Math.Round((low + high + eol) / 3m, MidpointRounding.AwayFromZero)
                };

                if (total > 0)
                {
                    result.LowPercent = Math.Round((decimal)low * 100m / total, 2);
                    result.HighPercent = Math.Round((decimal)high * 100m / total, 2);
                    result.EndOfLifePercent = Math.Round((decimal)eol * 100m / total, 2);
                }

                return result;
            }
        }

        public StateOfChargeModel GetStateOfCharge(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => (communityID == null ? true : b.Community.ID == communityID) && b.IsDeactivated == false)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        GroupID = (long?)b.Group.ID
                    })
                    .ToList();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    var includedGroupIds = allChildrenGroups.Select(g => g.ID).ToHashSet();
                    includedGroupIds.Add(groupID.Value);

                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID != null && includedGroupIds.Contains(x.GroupID.Value))
                        .ToList();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    batteryQuery = batteryQuery.Where(x => x.GroupID == groupID).ToList();
                }

                var allowedBatteryIds = batteryQuery.Select(x => x.BatteryID).ToHashSet();

                // Power Modules is the number of batteries in current filter.
                var powerModulesCount = allowedBatteryIds.Count;

                // SP already returns latest row per Battery_ID.
                var spRows = db.Database.SqlQuery<StateOfChargeRow>("EXEC " + StateOfChargeStoredProcedureName)
                    .ToList();

                var rows = spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList();

                var high = 0;
                var low = 0;
                var chargeNow = 0;
                var socSum = 0;

                foreach (var row in rows)
                {
                    var soc = row.SlowChangingDataA_RelativeStateOfCharge;
                    socSum += soc;

                    if (soc >= 80)
                    {
                        high++;
                    }
                    else if (soc >= 60)
                    {
                        low++;
                    }
                    else
                    {
                        chargeNow++;
                    }
                }

                var total = high + low + chargeNow;

                var result = new StateOfChargeModel
                {
                    HighSoCCount = high,
                    LowSoCCount = low,
                    ChargeNowCount = chargeNow,
                    TotalCount = total,
                    PowerModulesCount = powerModulesCount,
                    AverageSoC = total > 0 ? (int)Math.Round((decimal)socSum / total, MidpointRounding.AwayFromZero) : 0
                };

                if (total > 0)
                {
                    result.HighSoCPercent = Math.Round((decimal)high * 100m / total, 2);
                    result.LowSoCPercent = Math.Round((decimal)low * 100m / total, 2);
                    result.ChargeNowPercent = Math.Round((decimal)chargeNow * 100m / total, 2);
                }

                return result;
            }
        }

        #endregion

        #region AdminDashboard
        public AdminDashboardModel GetAdminDashboard()
        {
            AdminDashboardModel model = new AdminDashboardModel();
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                model.Communities = db.Communities.Count();
                model.Groups = db.Groups.Count();
                using (var userdb = new ApplicationDbContext())
                {
                    model.Users = userdb.Users.Count();
                }
                model.RegDevices = db.Devices.Where(x => x.Community != null).Count();
                model.RemDevices = db.Devices.Where(x => x.Community == null).Count();

                model.RegBatteries = db.Batteries.Where(x => x.Community != null).Count();
                model.RemBatteries = db.Batteries.Where(x => x.Community == null).Count();
                return model;
            }
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}