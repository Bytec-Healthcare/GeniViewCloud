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
    public partial class GeniViewCloudDataRepository : DbContext
    {
        public GeniViewCloudDataRepository() : base("name=GeniViewCloudDataRepository")
        {
           Database.SetInitializer(new GeniViewCloudDataRepositoryInitializer());
        }

        public virtual DbSet<Agent> Agents { get; set; }
        public virtual DbSet<Battery> Batteries { get; set; }
        public virtual DbSet<AgentBatteryLog> AgentBatteryLog { get; set; }
        public virtual DbSet<InternalBatteryLog> InternalBatteryLog { get; set; }
        public virtual DbSet<InternalDeviceLog> InternalDeviceLog { get; set; }
        public virtual DbSet<Community> Communities { get; set; }
        public virtual DbSet<AgentDeviceLog> AgentDeviceLog { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DeviceEvent> DeviceEvents { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        
        public virtual DbSet<MailServer> MailServer { get; set; }
        public virtual DbSet<DeviceEventNotification> DeviceEventActionNotifications { get; set; }
        public virtual DbSet<ApplicationUpdate> ApplicationUpdates { get; set; }
        public virtual DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public virtual DbSet<UserActivityHistory> UserActivityHistory { get; set; }
    }
}