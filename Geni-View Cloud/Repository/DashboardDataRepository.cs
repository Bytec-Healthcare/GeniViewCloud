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

        private sealed class CycleStatusRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public int OperatingData_CycleCount { get; set; }
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

                // SP already returns latest row per Battery_ID.
                var spRows = db.Database.SqlQuery<CycleStatusRow>("EXEC " + CycleStatusStoredProcedureName)
                    .ToList();

                var rows = spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList();

                var active = 0;
                var idle = 0;
                var svc = 0;

                foreach (var row in rows)
                {
                    if (row.OperatingData_CycleCount < 10)
                    {
                        active++;
                    }
                    else if (row.OperatingData_CycleCount <= 20)
                    {
                        idle++;
                    }
                    else
                    {
                        svc++;
                    }
                }

                var total = active + idle + svc;

                var result = new CycleStatusModel
                {
                    ActiveCount = active,
                    IdleCount = idle,
                    SvcCount = svc,
                    TotalCount = total
                };

                if (total > 0)
                {
                    result.ActivePercent = Math.Round((decimal)active * 100m / total, 2);
                    result.IdlePercent = Math.Round((decimal)idle * 100m / total, 2);
                    result.SvcPercent = Math.Round((decimal)svc * 100m / total, 2);
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