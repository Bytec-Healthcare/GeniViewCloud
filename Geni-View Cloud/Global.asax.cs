using GeniView.Cloud.Areas.Admin.Models;
using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GeniView.Cloud
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            // Set connection string for log manager. This is to avoid repeating connection string multiple times, and keeping it only in web config.
            var dataConnString = WebConfigurationManager.ConnectionStrings["GeniViewCloudDataRepository"].ConnectionString;
            LogManager.Configuration.Variables["GeniViewCloudDataRepository"] = dataConnString;

            _logger.Info($"Geni-View Cloud version {Assembly.GetExecutingAssembly().GetName().Version} has started.");

            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ViewEngines.Engines.Add(new NewPartialViewEngine());
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            Database.SetInitializer<ApplicationDbContext>
                (
                    new MigrateDatabaseToLatestVersion<ApplicationDbContext, GeniView.Cloud.Migrations.Configuration>()
                );



            HangfireBootstrapper.Instance.Start();

            // MQTT Client

            Task.Factory.StartNew(() => {
                _logger.Info("Wait 10 sec satrt mqtt client.");
                Thread.Sleep(10000);
                MQTTHelper.Instance.Connect();

            });
            //MQTTHelper.Instance.Connect();
        }

        protected void Application_End()
        {
            _logger.Info($"Geni-View Cloud has ended.");
            HangfireBootstrapper.Instance.Stop();

            // MQTT Client
            MQTTHelper.Instance.Dispose();
        }
    }
}
