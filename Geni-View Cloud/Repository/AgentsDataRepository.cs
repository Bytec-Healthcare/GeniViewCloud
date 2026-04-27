using GeniView.Cloud.Models;
using GeniView.Data.Agent;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class AgentsDataRepository : IDisposable
    {
        private readonly GeniViewCloudDataRepository _db;

        public AgentsDataRepository(GeniViewCloudDataRepository db)
        {
            _db = db;
        }

        #region Agents
        public List<AgentViewModel> GetAgents()
        {
            var db = _db;
            {

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