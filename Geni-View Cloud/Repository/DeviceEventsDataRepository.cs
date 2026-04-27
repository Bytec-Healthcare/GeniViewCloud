using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class DeviceEventsDataRepository : IDisposable
    {
        private readonly DBHelper _dbHelper = new DBHelper();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly GeniViewCloudDataRepository _db;

        public DeviceEventsDataRepository(GeniViewCloudDataRepository db)
        {
            _db = db;
        }

        public bool CreateBatch(List<DeviceEvent> events, GeniViewCloudDataRepository db)
        {
            if (events == null || events.Count == 0)
                return true;

            _dbHelper.BatchInsert(db, db.DeviceEvents, events);
            return true;
        }

        public List<DeviceEvent> GetLatestDeviceEvents(long? communityID, long? groupID, bool includeAllSubGroups, int count)
        {
            var db = _db;
            {

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
                    var groupDb = new GroupsDataRepository(_db);
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