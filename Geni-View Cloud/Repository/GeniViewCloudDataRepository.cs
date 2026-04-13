using GeniView.Cloud.Models;
using GeniView.Cloud.Common;
using GeniView.Data.Agent;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public partial class GeniViewCloudDataRepository : DbContext
    {
        public GeniViewCloudDataRepository(DbContextOptions<GeniViewCloudDataRepository> options)
            : base(options)
        {
        }

        // Parameterless constructor for use in repositories until DI is fully wired in Phase 3.
        // TODO Phase 3: remove this and inject via IServiceProvider in all callers.
        public GeniViewCloudDataRepository()
            : base(new DbContextOptionsBuilder<GeniViewCloudDataRepository>().Options)
        {
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

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // With <Nullable>disable</Nullable>, EF Core 8 treats every reference-type
            // [ComplexType] property as optional, which it does not support at any nesting
            // depth. This convention walks the whole type hierarchy and marks them all required.
            configurationBuilder.Conventions.Add(_ => new RequiredComplexPropertiesConvention());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // AppContext.BaseDirectory always resolves to the app's deployment folder.
                // Directory.GetCurrentDirectory() returns C:\Windows\System32 under IIS on .NET 8,
                // which caused appsettings.json to not be found and the connection string to be null.
                var basePath = !string.IsNullOrEmpty(Global._serverPath)
                    ? Global._serverPath
                    : AppContext.BaseDirectory;

                var config = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("GeniViewCloudDataRepository"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EF6 used PluralizingTableNameConvention (entity class name → plural table name).
            // EF Core uses the DbSet property name as-is. Map each mismatched table explicitly.
            modelBuilder.Entity<AgentDeviceLog>().ToTable("AgentDeviceLogs");
            modelBuilder.Entity<AgentBatteryLog>().ToTable("AgentBatteryLogs");
            modelBuilder.Entity<InternalBatteryLog>().ToTable("InternalBatteryLogs");
            modelBuilder.Entity<InternalDeviceLog>().ToTable("InternalDeviceLogs");
            modelBuilder.Entity<MailServer>().ToTable("MailServers");
            modelBuilder.Entity<DeviceEventNotification>().ToTable("DeviceEventNotifications");
            modelBuilder.Entity<UserActivityHistory>().ToTable("UserActivityHistories");

            // Force Battery and Device FK names to EF6 schema columns.
            modelBuilder.Entity<Battery>(b =>
            {
                b.Property(x => x.CommunityID).HasColumnName("Community_ID");
                b.Property(x => x.GroupID).HasColumnName("Group_ID");

                b.HasOne(x => x.Community)
                    .WithMany(x => x.Batteries)
                    .HasForeignKey(x => x.CommunityID);

                b.HasOne(x => x.Group)
                    .WithMany(x => x.Batteries)
                    .HasForeignKey(x => x.GroupID);
            });

            modelBuilder.Entity<Device>(d =>
            {
                d.Property(x => x.CommunityID).HasColumnName("Community_ID");
                d.Property(x => x.GroupID).HasColumnName("Group_ID");

                d.HasOne(x => x.Community)
                    .WithMany(x => x.Devices)
                    .HasForeignKey(x => x.CommunityID);

                d.HasOne(x => x.Group)
                    .WithMany(x => x.Devices)
                    .HasForeignKey(x => x.GroupID);
            });

            // Force BatterySettings FK names to EF6 schema columns.
            modelBuilder.Entity<BatterySettings>(b =>
            {
                b.Property(x => x.Battery_ID).HasColumnName("Battery_ID");
                b.Property(x => x.Agent_ID).HasColumnName("Agent_ID");

                b.HasOne(x => x.Battery)
                    .WithMany(x => x.BatterySettingsCollection)
                    .HasForeignKey(x => x.Battery_ID);

                b.HasOne(x => x.Agent)
                    .WithMany()
                    .HasForeignKey(x => x.Agent_ID);

                b.Ignore("BatteryID");
                b.Ignore("AgentID");
            });

            // Force DeviceSettings FK names to EF6 schema columns.
            modelBuilder.Entity<DeviceSettings>(d =>
            {
                d.Property(x => x.Device_ID).HasColumnName("Device_ID");
                d.Property(x => x.Agent_ID).HasColumnName("Agent_ID");

                d.HasOne(x => x.Device)
                    .WithMany(x => x.DeviceSettingsCollection)
                    .HasForeignKey(x => x.Device_ID);

                d.HasOne(x => x.Agent)
                    .WithMany()
                    .HasForeignKey(x => x.Agent_ID);

                d.Ignore("DeviceID");
                d.Ignore("AgentID");
            });

            // Force underscore FK names for runtime log entities.
            modelBuilder.Entity<InternalBatteryLog>(e =>
            {
                e.Property(x => x.Battery_ID).HasColumnName("Battery_ID");
                e.Property(x => x.Agent_ID).HasColumnName("Agent_ID");

                e.HasOne(x => x.Battery)
                    .WithMany(b => b.InternalBatteryLogCollection)
                    .HasForeignKey(x => x.Battery_ID);

                e.HasOne(x => x.Agent)
                    .WithMany()
                    .HasForeignKey(x => x.Agent_ID);

                e.Ignore("BatteryID");
                e.Ignore("AgentID");
            });

            modelBuilder.Entity<AgentBatteryLog>(e =>
            {
                e.Property(x => x.Battery_ID).HasColumnName("Battery_ID");
                e.Property(x => x.Agent_ID).HasColumnName("Agent_ID");

                e.HasOne(x => x.Battery)
                    .WithMany(b => b.AgentBatteryLogCollection)
                    .HasForeignKey(x => x.Battery_ID);

                e.HasOne(x => x.Agent)
                    .WithMany()
                    .HasForeignKey(x => x.Agent_ID);

                e.Ignore("BatteryID");
                e.Ignore("AgentID");
            });

            modelBuilder.Entity<InternalDeviceLog>(e =>
            {
                e.Property(x => x.Device_ID).HasColumnName("Device_ID");
                e.Property(x => x.Agent_ID).HasColumnName("Agent_ID");

                e.HasOne(x => x.Device)
                    .WithMany(d => d.InternalDeviceLogCollection)
                    .HasForeignKey(x => x.Device_ID);

                e.HasOne(x => x.Agent)
                    .WithMany()
                    .HasForeignKey(x => x.Agent_ID);

                e.Ignore("DeviceID");
                e.Ignore("AgentID");
            });

            modelBuilder.Entity<AgentDeviceLog>(e =>
            {
                e.Property(x => x.Device_ID).HasColumnName("Device_ID");
                e.Property(x => x.Agent_ID).HasColumnName("Agent_ID");

                e.HasOne(x => x.Device)
                    .WithMany(d => d.AgentDeviceLogCollection)
                    .HasForeignKey(x => x.Device_ID);

                e.HasOne(x => x.Agent)
                    .WithMany()
                    .HasForeignKey(x => x.Agent_ID);

                e.Ignore("DeviceID");
                e.Ignore("AgentID");
            });

            modelBuilder.Entity<DeviceEvent>(e =>
            {
                e.Property(x => x.Device_ID).HasColumnName("Device_ID");
                e.Property(x => x.Agent_ID).HasColumnName("Agent_ID");

                e.HasOne(x => x.Device)
                    .WithMany(d => d.DeviceEventCollection)
                    .HasForeignKey(x => x.Device_ID);

                e.HasOne(x => x.Agent)
                    .WithMany()
                    .HasForeignKey(x => x.Agent_ID);

                e.Ignore("DeviceID");
                e.Ignore("AgentID");
            });
        }

        // Marks every [ComplexType] property as required at any nesting depth.
        // RequiredComplexPropertiesConvention handles the case where <Nullable>disable</Nullable>
        // causes EF Core 8 to treat all reference-type complex properties as optional (unsupported).
        private sealed class RequiredComplexPropertiesConvention : IModelFinalizingConvention
        {
            public void ProcessModelFinalizing(
                IConventionModelBuilder modelBuilder,
                IConventionContext<IConventionModelBuilder> context)
            {
                foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
                {
                    MakeComplexPropertiesRequired(entityType);
                }
            }

            private static void MakeComplexPropertiesRequired(IConventionTypeBase typeBase)
            {
                foreach (var cp in typeBase.GetMembers().OfType<IConventionComplexProperty>())
                {
                    cp.Builder.IsRequired(true);
                    MakeComplexPropertiesRequired(cp.ComplexType);
                }
            }
        }
    }
}
