using GeniView.Cloud.Models;
using GeniView.Data.Agent;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Repository
{
    public class AgentsDataRepository : IDisposable
    {
        #region Agents
        public List<AgentViewModel> GetAgents()
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var mainQuery = (from a in db.Agents
                                 select new AgentViewModel()
                                 {
                                     Agent = a,
                                     Status = (a.Timestamp >= GlobalSettings.OnlineRangeInMinutes) ? new ExtraInfo { Name = "Online", Color = GlobalSettings.SuccessColor } :
                                              (a.Timestamp >= GlobalSettings.OfflineRangeInDays && a.Timestamp < GlobalSettings.OnlineRangeInMinutes) ? new ExtraInfo { Name = "Offline", Color = GlobalSettings.WarningColor } :
                                              new ExtraInfo { Name = "Unknown", Color = GlobalSettings.AlertColor },
                                 }).ToList();
                return mainQuery;
            }

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}