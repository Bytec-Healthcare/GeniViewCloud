using GeniView.Cloud.Repository;
using Hangfire;
using Hangfire.SqlServer;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.Hosting;


namespace GeniView.Cloud.Common
{
    [ExcludeFromCodeCoverage]
    public class HangfireBootstrapper : IRegisteredObject
    {
        HangfireRepository _hangfire = new HangfireRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();


        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        private readonly object _lockObject = new object();
        private bool _started;

        private BackgroundJobServer _backgroundJobServer;

        private HangfireBootstrapper()
        {
        }

        public void Start()
        {
            if (Global._enableHangFire == true)
            {
                lock (_lockObject)
                {
                    _logger.Info($"HangfireBootstrapper Start {Global._scanDevice}");

                    if (_started) return;
                    _started = true;

                    HostingEnvironment.RegisterObject(this);

                    //SQL 
                    var sqloptions = new SqlServerStorageOptions()
                    {
                        QueuePollInterval = TimeSpan.Zero
                    };


                    GlobalConfiguration.Configuration.UseSqlServerStorage(
                        _hangfire.Database.Connection.ConnectionString,
                        sqloptions
                    );

                    var options = new BackgroundJobServerOptions()
                    {
                        SchedulePollingInterval = TimeSpan.FromMilliseconds(100)
                    };

                    //Setting jobs
                    HFScheduler hfscheduler = new HFScheduler();
                    hfscheduler.Setting();

                    _backgroundJobServer = new BackgroundJobServer(options);
                }
            }
        }

        public void Stop()
        {
            if (Global._enableHangFire == false)
            {
                lock (_lockObject)
                {
                    _logger.Info($"HangfireBootstrapper Stop");

                    if (_backgroundJobServer != null)
                    {
                        _backgroundJobServer.Dispose();
                    }

                    HostingEnvironment.UnregisterObject(this);
                }
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            Stop();
        }
    }
}