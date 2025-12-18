using GeniView.Cloud.Models;
using GeniView.Cloud.PowerBI;
using GeniView.Data.Hardware.Event;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace GeniView.Cloud.Repository
{
    // WARNING : CHANGING THIS WILL CAUSE TO LOOSE ALL DATA
    public class GeniViewCloudDataRepositoryInitializer : IDatabaseInitializer<GeniViewCloudDataRepository>
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public void InitializeDatabase(GeniViewCloudDataRepository context)
        {
            try
            {
                if (!context.Database.Exists())
                {
                    context.Database.Create();
                    Seed(context);
                }
                else if (context.Database.CompatibleWithModel(true))
                {
                    Seed(context);
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Database creation failed.", ex);
            }
        }

        private void Seed(GeniViewCloudDataRepository context)
        {
            // Populate device events.
            foreach (var item in DeviceEventNotification.Seed())
            {
                if (!context.DeviceEventActionNotifications.Any(x => x.UID == item.UID))
                    context.DeviceEventActionNotifications.Add(item);
            }

            // Populate application update entires.
            foreach (var item in ApplicationUpdate.Seed())
            {
                if (!context.ApplicationUpdates.Any(x => x.AppId == item.AppId))
                    context.ApplicationUpdates.Add(item);
            }

            // Create default agent for G3 flow
            var findAgent = context.Agents.Where(a => a.Name.ToLower() == "default").FirstOrDefault();
            if (findAgent == null)
            {
                var defaultAgent = Data.Agent.Agent.Default();
                context.Agents.Add(defaultAgent);
            }

            try
            {
                context.Database.ExecuteSqlCommand(StoredProcedures.AgentBatteryLogsWithDurationView, new object[0]);
                context.Database.ExecuteSqlCommand(StoredProcedures.AgentDeviceLogsWithDurationView,new object[0]);
                context.Database.ExecuteSqlCommand(StoredProcedures.InternalBatteryLogsWithDurationView, new object[0]);
                context.Database.ExecuteSqlCommand(StoredProcedures.InternalDeviceLogsWithDurationView, new object[0]);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot create sql views.", ex);
            }
            context.SaveChanges();
        }
    }
}