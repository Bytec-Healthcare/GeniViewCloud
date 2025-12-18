using GeniView.Cloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Repository
{
    public class ApplicationLogsDataRepository : IDisposable
    {
        public List<ApplicationLog> GetApplicationLogs(ApplicationLogsFilter filter, ApplicationUser currentUser)
        {
            List<ApplicationLog> model = new List<ApplicationLog>();
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                string searchLevel = filter.LogLevel == ApplicationLogLevel.ALL ? "" : filter.LogLevel.ToString();
                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(filter.BeginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(filter.EndDate, currentUser);

                model = db.ApplicationLogs.Where(x => x.Logged >= convertedBeginDate && 
                                                      x.Logged <= convertedEndDate &&
                                                      x.Level.Contains(searchLevel)
                                                ).OrderByDescending(x => x.Logged)
                                                 .Take(filter.Count)
                                                 .ToList();

                return model;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}