using GeniView.Cloud.Common;
using GeniView.Data.Hardware.Event;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Repository
{
    public class DeviceEventRepository
    {
        private DBHelper _dbHelper = new DBHelper();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public bool CreateBatch (List<DeviceEvent>events, GeniViewCloudDataRepository db)
        {
            var ret = false;
            _dbHelper.BatchInsert(db, db.DeviceEvents, events);
            ret = true;
            return ret;
        }


    }
}