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
    public class HangfireRepositoryInitializer : IDatabaseInitializer<HangfireRepository>
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public void InitializeDatabase(HangfireRepository context)
        {
            try
            {
                if (!context.Database.Exists())
                {
                    context.Database.Create();
                }
                else if (context.Database.CompatibleWithModel(true))
                {
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Hangfire database creation failed.", ex);
            }
        }

    }
}