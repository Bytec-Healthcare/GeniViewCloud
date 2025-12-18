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
        private Random randomDouble = new Random();

        #region Dashboard
        public DeviceDashboardModel GetDeviceDashboardChartData(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            DeviceDashboardModel model = new DeviceDashboardModel();
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var mainDevicesQuery = (from d in db.Devices
                                        where (communityID == null ? true : d.Community.ID == communityID) && (d.IsDeactivated == false)
                                        select new
                                        {
                                            DeviceID = d.ID,
                                            GroupID = (long?)d.Group.ID,
                                            LastRow = d.AgentDeviceLogCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                                        }).AsEnumerable();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }
                    mainDevicesQuery = (from m in mainDevicesQuery.Where(x => x.GroupID != null)
                                        join ch in allChildrenGroups on m.GroupID equals ch.ID
                                        select m).AsEnumerable();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    mainDevicesQuery = mainDevicesQuery.Where(x => x.GroupID == groupID);
                }

                model.DeviceOnlineOnBattery = (from m in mainDevicesQuery
                                                   //where (m.LastRow != null && m.LastRow.Timestamp >= GlobalSettings.OnlineRangeInMinutes && m.LastRow.IsExternalPowerInputApplied == false)
                                               where (m.LastRow != null && m.LastRow.Timestamp >= GlobalSettings.OnlineRangeInMinutes )

                                               select m).Count();

                //model.DeviceOnlinePluggedIn = (from m in mainDevicesQuery
                //                               where (m.LastRow != null && m.LastRow.Timestamp >= GlobalSettings.OnlineRangeInMinutes && m.LastRow.IsExternalPowerInputApplied == true)
                //                               select m).Count();

                model.DeviceOffline = (from m in mainDevicesQuery
                                       where (m.LastRow != null && m.LastRow.Timestamp >= GlobalSettings.OfflineRangeInDays && m.LastRow.Timestamp < GlobalSettings.OnlineRangeInMinutes)
                                       select m).Count();

                model.DeviceUnknown = (from m in mainDevicesQuery
                                       where (m.LastRow != null && m.LastRow.Timestamp < GlobalSettings.OfflineRangeInDays)
                                       select m).Count();
                return model;
            }

        }

        public BatteryDashboardModel GetBatteryDashboardChartData(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            BatteryDashboardModel model = new BatteryDashboardModel();

            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                //db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                //var mainBatteriesQuery = (from b in db.Batteries
                //                          where (communityID == null ? true : b.Community.ID == communityID) && (b.IsDeactivated == false)
                //                          select new
                //                          {
                //                              BatteryID = b.ID,
                //                              GroupID = (long?)b.Group.ID,
                //                              LastRow = b.AgentBatteryLogCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                //                          }).ToList();


                var mainBatteriesQuery = (from b in db.Batteries
                                          where (communityID == null ? true : b.Community.ID == communityID) && (b.IsDeactivated == false)
                                          select new
                                          {
                                              BatteryID = b.ID,
                                              GroupID = (long?)b.Group.ID,
                                              LastRow = (from l in b.AgentBatteryLogCollection
                                                         where l.Battery_ID == b.ID
                                                         orderby l.Timestamp descending
                                                         select l).FirstOrDefault()
                                          }).ToList();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }
                    mainBatteriesQuery = (from m in mainBatteriesQuery.Where(x => x.GroupID != null)
                                          join ch in allChildrenGroups on m.GroupID equals ch.ID
                                          select m).AsEnumerable().ToList();

                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    mainBatteriesQuery = mainBatteriesQuery.Where(x => x.GroupID == groupID).ToList();
                }

                // We will Use later for calculating other values
                var onlineDischargingBatteries = from m in mainBatteriesQuery
                                                 let row = m.LastRow
                                                 where (row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes && row.Status == BatteryStates.PoweringSystem)
                                                 select m;

                model.BatteryDischarging = onlineDischargingBatteries.Count();

                var onlineChargingBatteries = from m in mainBatteriesQuery
                                              let row = m.LastRow
                                              where (row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes && row.Status == BatteryStates.Charging)
                                              select m;

                model.BatteryCharging = onlineChargingBatteries.Count();

                // Check Count Discharging Batteries
                // Note : If count = 0 skip calculation, because division by zero and null exception.
                if (model.BatteryDischarging > 0)
                {
                    model.PowerUsageStateOfCharge = onlineDischargingBatteries.Sum(x => x.LastRow.SlowChangingDataA.RelativeStateOfCharge) / model.BatteryDischarging;
                    model.PowerUsageCapacity = Math.Round(onlineDischargingBatteries.Sum(x => x.LastRow.SlowChangingDataA.RemainingCapacity) * GlobalSettings.NominalVoltage / 1000, 2);

                    double powerConsumptionSum = (from c in onlineDischargingBatteries
                                                  let row = c.LastRow
                                                  select new { b = (row.OperatingData.AverageCurrent * row.OperatingData.Voltage) }
                                                  ).Sum(x => x.b);

                    model.PowerUsageConsumption = Math.Abs(Math.Round((powerConsumptionSum / model.BatteryDischarging) / 1000, 2)); // According to Document divide to 1000

                }

                model.BatteryNeedsCharging = (from m in mainBatteriesQuery
                                              let row = m.LastRow
                                              where (row != null && row.Timestamp >= GlobalSettings.OfflineRangeInDays
                                                                 && (row.Timestamp < GlobalSettings.OnlineRangeInMinutes
                                                                 || (row.Status != BatteryStates.Charging && row.Status != BatteryStates.PoweringSystem))
                                                                 && row.SlowChangingDataA.RelativeStateOfCharge < GlobalSettings.IsStateOfChargeReadyToUse)
                                              select m).Count();

                model.BatteryReadyToUse = (from m in mainBatteriesQuery
                                           let row = m.LastRow
                                           where (row != null && row.Timestamp >= GlobalSettings.OfflineRangeInDays
                                                              && (row.Timestamp < GlobalSettings.OnlineRangeInMinutes
                                                              || (row.Status != BatteryStates.Charging && row.Status != BatteryStates.PoweringSystem))
                                                              && row.SlowChangingDataA.RelativeStateOfCharge >= GlobalSettings.IsStateOfChargeReadyToUse)
                                           select m).Count();

                model.BatteryUnknown = (from m in mainBatteriesQuery
                                        let row = m.LastRow
                                        where (row != null && row.Timestamp < GlobalSettings.OfflineRangeInDays)
                                        select m).Count();

                if (model.BatteryCharging > 0)
                {
                    model.PowerAvailableCapacity = Math.Round(onlineChargingBatteries.Sum(x => x.LastRow.SlowChangingDataA.RemainingCapacity) * GlobalSettings.NominalVoltage / 1000, 2);
                    model.PowerAvailableStateOfCharge = onlineChargingBatteries.Sum(x => x.LastRow.SlowChangingDataA.RelativeStateOfCharge) / model.BatteryCharging;
                }

            }

            return model;
        }

        public IEnumerable<OnlineItemsChartModel> GetBatteryOnlineChartData0(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                List<OnlineItemsChartModel> modelBattery = new List<OnlineItemsChartModel>();

                var mainBatteriesQuery = db.AgentBatteryLog
                                           .Include(x => x.Battery)
                                           .Where(x => 
                                           (communityID != null ? x.Battery.Community.ID == communityID : true) 
                                           && x.Timestamp > GlobalSettings.OfflineRangeInDays 
                                           && x.Battery.IsDeactivated == false)
                                           .AsNoTracking()
                                           .Select(t => new
                                           {
                                               Timestamp = t.Timestamp,
                                               BatteryID = t.Battery.ID,
                                               Group = t.Battery.Group
                                           });

                // Global Filter
                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }
                    mainBatteriesQuery = (from m in mainBatteriesQuery.Where(x => x.Group != null)
                                          join ch in allChildrenGroups on m.Group.ID equals ch.ID
                                          select m);
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    mainBatteriesQuery = mainBatteriesQuery.Where(x => x.Group.ID == groupID);
                }

                modelBattery = mainBatteriesQuery.ToList()
                               .GroupBy(g => new
                               {
                                   g.Timestamp.Date,
                                   g.BatteryID,
                               })
                               .Select(x => new { Date = x.Key.Date })
                               .GroupBy(grp => new { grp.Date })
                               .Select(x => new OnlineItemsChartModel
                               {
                                   Date = x.Key.Date.ToString(Global.dateFormat),
                                   //Date = x.Key.Date.ToString(CultureInfo.InvariantCulture),
                                   OnlineCount = x.Count()
                               }).ToList();

                return modelBattery;
            }
        }

        public IEnumerable<OnlineItemsChartModel> GetBatteryOnlineChartData(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                //var query = db.AgentBatteryLog
                //    .Where(x => x.Timestamp > GlobalSettings.OfflineRangeInDays
                //             && x.Battery.IsDeactivated == false)
                //    .AsNoTracking();

                //var query = db.AgentBatteryLog
                //    .Where(x => x.Timestamp > GlobalSettings.OfflineRangeInDays
                //             && x.Battery.IsDeactivated == false)
                //    .AsNoTracking()
                //    //.Select(x => new
                //    //{
                //    //    BatteryID = x.Battery.ID,
                //    //    Timestamp = x.Timestamp,
                //    //    Date = DbFunctions.TruncateTime(x.Timestamp)
                //    //})
                //    .GroupBy(x => new { x.Timestamp, x.Battery_ID });
                //    //.Select(g => g.OrderBy(x => x.Timestamp).FirstOrDefault());


                //Reduce data scale then get the last log
                var maxTimes = db.AgentBatteryLog.AsNoTracking()
                        .Where(x => x.Timestamp > GlobalSettings.OfflineRangeInDays
                                 && x.Battery.IsDeactivated == false)
                    .GroupBy(log => new
                    {
                        log.Battery_ID,
                        Date = DbFunctions.TruncateTime(log.Timestamp)
                    })
                    .Select(g => new
                    {
                        g.Key.Battery_ID,
                        g.Key.Date,
                        MaxTs = g.Max(x => x.Timestamp)
                    });

                var query = from log in db.AgentBatteryLog.AsNoTracking()
                                .Where(x => x.Timestamp > GlobalSettings.OfflineRangeInDays
                                         && x.Battery.IsDeactivated == false)
                            join mt in maxTimes
                                on new
                                {
                                    log.Battery_ID,
                                    Date = DbFunctions.TruncateTime(log.Timestamp),
                                    log.Timestamp
                                }
                                equals new
                                {
                                    mt.Battery_ID,
                                    Date = mt.Date,
                                    Timestamp = mt.MaxTs
                                }
                            select log;

                if (communityID != null)
                {
                    query = query.Where(x => x.Battery.Community.ID == communityID);
                }

                List<long> includedGroupIDs = new List<long>();
                if (groupID != null)
                {
                    if (includeAllSubGroups)
                    {
                        using (var groupdb = new GroupsDataRepository())
                        {
                            var allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                            includedGroupIDs = allChildrenGroups.Select(g => g.ID).ToList();
                        }
                        if (!includedGroupIDs.Contains(groupID.Value))
                        {
                            includedGroupIDs.Add(groupID.Value);
                        }
                    }
                    else
                    {
                        includedGroupIDs.Add(groupID.Value);
                    }
                }

                if (includedGroupIDs.Any())
                {
                    query = query.Where(x => includedGroupIDs.Contains(x.Battery.Group.ID));
                }

                var test = query
                    .Select(x=> new {ID =x.Battery_ID , TimeStamp = x.Timestamp }).ToList(); 

                var result = test
                    .Distinct()
                    .GroupBy(x => new { x.TimeStamp.Date , x.ID})
                    .Select(x => new { Date = x.Key.Date })
                    .GroupBy(grp => new { grp.Date })
                    .Select(g => new OnlineItemsChartModel
                    {
                        Date = g.Key.Date.ToString(Global.dateFormat),
                        OnlineCount = g.Count()
                    })
                    .ToList();

                return result;
            }
        }

        public IEnumerable<OnlineItemsChartModel> GetDeviceOnlineChartData(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                List<OnlineItemsChartModel> modelDevice = new List<OnlineItemsChartModel>();

                var mainDevicesQuery = db.AgentDeviceLog
                                         .Include(x => x.Device)
                                         .Include(x => x.Device.Group)
                                         .Where(x => (communityID != null ? x.Device.Community.ID == communityID : true) && x.Timestamp > GlobalSettings.OfflineRangeInDays && x.Device.IsDeactivated == false)
                                         .AsNoTracking()
                                         .Select(t => new
                                         {
                                             Timestamp = t.Timestamp,
                                             DeviceID = t.Device.ID,
                                             Group = t.Device.Group
                                         });

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    mainDevicesQuery = (from m in mainDevicesQuery.Where(x => x.Group != null)
                                        join c in allChildrenGroups on m.Group.ID equals c.ID
                                        select m);
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    mainDevicesQuery = mainDevicesQuery.Where(x => x.Group.ID == groupID);
                }

                modelDevice = mainDevicesQuery
                                  .AsEnumerable()
                                  .GroupBy(g => new
                                  {
                                      g.Timestamp.Date,
                                      g.DeviceID,
                                  })
                                  .Select(x => new { Date = x.Key.Date })
                                  .GroupBy(grp => new { grp.Date })
                                  .Select(x => new OnlineItemsChartModel
                                  {
                                      //Date = x.Key.Date.ToString(CultureInfo.InvariantCulture),
                                      Date = x.Key.Date.ToString(Global.dateFormat),
                                      OnlineCount = x.Count()
                                  }).ToList();

                return modelDevice;
            }
        }

        public List<DeviceEvent> GetDeviceEventHistory(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            List<DeviceEvent> model = new List<DeviceEvent>();
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var mainBatteriesQuery = db.DeviceEvents
                                           .Include(x => x.Device)
                                           .Include(x => x.Agent)
                                           .Where(x => (communityID != null ? x.Device.Community.ID == communityID : true) &&
                                                        x.Timestamp > GlobalSettings.OfflineRangeInDays &&
                                                        x.Device.IsDeactivated == false)
                                           .AsNoTracking()
                                           .Select(t => new
                                           {
                                               DeviceEvent = t,
                                               //DeviceID = t.Device.ID,
                                               Group = t.Device.Group
                                           });


                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }
                    mainBatteriesQuery = (from m in mainBatteriesQuery.Where(x => x.Group != null)
                                          join ch in allChildrenGroups on m.Group.ID equals ch.ID
                                          select m);
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    mainBatteriesQuery = mainBatteriesQuery.Where(x => x.Group != null && x.Group.ID == groupID);
                }

                model = mainBatteriesQuery.Select(x => x.DeviceEvent).Take(50).OrderByDescending(x => x.Timestamp).ToList();

                return model;
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

        #region Agent Location
        public List<DeviceLocationViewModel> GetDevicesLocation(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            List<DeviceLocationViewModel> model = new List<DeviceLocationViewModel>();

            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                var mainDevicesQuery = (from d in db.Devices.Include(x => x.AgentDeviceLogCollection)
                                                            .Include(x => x.Group)
                                                            .Include(x => x.Community)
                                                            .Include(x => x.DeviceSettings)
                                        where (communityID == null ? true : d.Community.ID == communityID) && (d.IsDeactivated == false)
                                        let row = d.AgentDeviceLogCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                                        let lastsetting = d.DeviceSettingsCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                                        select new
                                        {
                                            Device = d,
                                            row,
                                            lastsetting
                                        }).AsEnumerable();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    mainDevicesQuery = (from m in mainDevicesQuery.Where(x => x.Device.Group != null)
                                        join ch in allChildrenGroups on m.Device.Group.ID equals ch.ID
                                        select m).AsEnumerable();
                }
                else if (groupID != null && !includeAllSubGroups)//Get Exact one Group assigned Devices
                {
                    mainDevicesQuery = mainDevicesQuery.Where(x => x.Device.Group.ID == groupID);
                }

                if (mainDevicesQuery.ToList() != null)
                {
                    model = (from m in mainDevicesQuery
                             let calcLatLong = (m.row != null) ? getDecimalCount(m.row.Location.Longitude, m.row.Location.Lattitude, 50) : new LocationLatLong { Lattitude = 0, Longitude = 0 }
                             select new DeviceLocationViewModel()
                             {
                                 SerialNumber = m.Device.SerialNumber,
                                 UserName = m.lastsetting != null ? m.lastsetting.UserInformation.Name : "No Info",
                                 Longitude = calcLatLong.Longitude,
                                 Lattitude = calcLatLong.Lattitude,
                                 IsUnknown = (m.row != null) ? m.row.Location.IsUnknown : true,
                                 Status = (m.row != null && m.row.Timestamp >= GlobalSettings.OnlineRangeInMinutes) ? new ExtraInfo { Name = "Online", Color = GlobalSettings.SuccessColor } :
                                          (m.row != null && m.row.Timestamp >= GlobalSettings.OfflineRangeInDays && m.row.Timestamp < GlobalSettings.OnlineRangeInMinutes) ? new ExtraInfo { Name = "Offline", Color = GlobalSettings.WarningColor } : new ExtraInfo { Name = "Unknown", Color = GlobalSettings.AlertColor },
                             }).ToList();
                }
                return model;
            }

        }
        // This method create new location around original location in radius 50 meters
        private LocationLatLong getDecimalCount(double lon, double lat, int radius = 50)
        {
            double radiusInDegrees = radius / 111000f;
            double u = randomDouble.NextDouble();
            double v = randomDouble.NextDouble();
            double w = radiusInDegrees * Math.Sqrt(u);
            double t = 2 * Math.PI * v;
            double x = w * Math.Cos(t);
            double y = w * Math.Sin(t);

            // Adjust the x-coordinate for the shrinking of the east-west distances
            double new_x = x / Math.Cos(lat);
            double foundLongitude = Math.Round(new_x + lon, 9);
            double foundLatitude = Math.Round(y + lat, 9);

            return new LocationLatLong
            {
                Longitude = foundLongitude,
                Lattitude = foundLatitude
            };

        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }



}