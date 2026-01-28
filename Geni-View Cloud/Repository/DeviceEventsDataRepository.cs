using GeniView.Cloud.Models;
using GeniView.Cloud.Common;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class DeviceEventsDataRepository : IDisposable
    {
        private readonly DBHelper _dbHelper = new DBHelper();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public bool CreateBatch(List<DeviceEvent> events, GeniViewCloudDataRepository db)
        {
            if (events == null || events.Count == 0)
                return true;

            _dbHelper.BatchInsert(db, db.DeviceEvents, events);
            return true;
        }

        public List<DeviceEvent> GetLatestDeviceEvents(long? communityID, long? groupID, bool includeAllSubGroups, int count)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var mainQuery = db.DeviceEvents
                    .Include(x => x.Device)
                    .Include(x => x.Agent)
                    .Where(x =>
                        (communityID != null ? x.Device.Community.ID == communityID : true) &&
                        x.Device.IsDeactivated == false)
                    .AsNoTracking()
                    .Select(t => new
                    {
                        DeviceEvent = t,
                        Group = t.Device.Group
                    })
                    .AsEnumerable();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupDb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupDb.GetGroups(communityID, groupID);
                    }

                    mainQuery = (from m in mainQuery.Where(x => x.Group != null)
                                 join ch in allChildrenGroups on m.Group.ID equals ch.ID
                                 select m).AsEnumerable();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    mainQuery = mainQuery.Where(x => x.Group != null && x.Group.ID == groupID);
                }

                return mainQuery
                    .Select(x => x.DeviceEvent)
                    .OrderByDescending(x => x.Timestamp)
                    .Take(count)
                    .ToList();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}