using GeniView.Cloud.Controllers.API;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace GeniView.Cloud.Common
{
    public class WebHost : IRegisteredObject
    {

        bool _debugEnable = true;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        LogApiController _logApiController = new LogApiController();

        public static bool IsRegister { get; private set; }

        public static bool IsRecycling { get; private set; }

        public  WebHost()
        {
            try
            {
                _logger.Info($"WebHost excute RegisterObject.");

                HostingEnvironment.RegisterObject(this);
                IsRegister = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

        }
        public void Stop(bool immediate)
        {
            try
            {
                _logger.Info($"WebHost excute Stop.");

                IsRecycling = true;
                IsRegister = false;
                //MQTTHelper.Instance.Disconnect();
                MQTTHelper.Instance.Dispose();

                _logApiController.FinishLog(CancellationToken.None);

                if (immediate == true)
                {
                    HostingEnvironment.UnregisterObject(this);
                    _logger.Info($"WebHost excute UnregisterObject.");
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }
        }
    }
}