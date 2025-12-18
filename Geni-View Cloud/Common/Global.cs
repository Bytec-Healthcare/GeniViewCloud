using GeniView.Cloud.Common.Queue;
using GeniView.Cloud.Models;
using NLog;
using RenityArtemis.Web.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace GeniView.Cloud.Common
{
    public static class Global
    {
        public static string dateFormat         = "yyyy-MM-dd";//24 hour format
        public static string dateTimeFormat     = "yyyy-MM-dd HH:mm:ss";//24 hour format
        public static string fileDateTime       = "yyyy-MM-dd_HH-mm-ss-fff";//24 hour format
        public static string dateTimeFormatfull = "yyyy-MM-dd HH:mm:ss:ffff";//24 hour format

        public static string _serverPath = HostingEnvironment.MapPath("~");
        public static string _otaPath = $@"Files\Device\displayboard.bin";
        //public static string _otaPath = $@"{_serverPath}Files\Device\";



        public static WebHost _webHost = new WebHost();
        //static public ConcurrentQueue<MqttApplicationMessage> _msgQueue = new ConcurrentQueue<MqttApplicationMessage>();
        public static ConcurrentQueue<MQTTData> _msgQueue = new ConcurrentQueue<MQTTData>();
        public static Logger _logger = LogManager.GetCurrentClassLogger();

        public static QueueHelp _queueHelp = new QueueHelp();
        public static bool _scanDevice = false;

        public static MemCacheHelper _memCacheHelper = new MemCacheHelper();

        //public static ConcurrentDictionary<string, LogRate> _logRate = new ConcurrentDictionary<string, LogRate>();


#if (DEBUG)
        public static bool _enableHangFire = false;

#else
        public static bool _enableHangFire = true;
#endif

        public static bool SetScanDevice(bool value)
        {
            _scanDevice = value;

            return _scanDevice;
        }

        public static void DebugPrintf(string msg, bool enable)
        {
#if (DEBUG)
            if (enable == true)
            {
                Debug.WriteLine(msg);
            }
#endif
        }

        public static MemCacheHelper GetMemCacheHelper()
        {
            return _memCacheHelper;
        }
    }
}