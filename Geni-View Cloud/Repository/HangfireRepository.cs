using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using GeniView.Data.Agent;
using GeniView.Cloud.Models;
using GeniView.Data.Hardware.Event;

namespace GeniView.Cloud.Repository
{
    public partial class HangfireRepository : DbContext
    {
        public HangfireRepository() : base("name=GeniViewCloudHangfireRepository")
        {
            Database.SetInitializer(new HangfireRepositoryInitializer());
            var ret = Database.CreateIfNotExists();
        }
    }
}