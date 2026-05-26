using GeniView.Cloud.Models;
using GeniView.Cloud.PowerBI;
using GeniView.Data.Hardware.Event;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    // WARNING: CHANGING THIS WILL CAUSE DATA LOSS
    // Called once at startup via Program.cs after EF Core migrations are applied.
    public class GeniViewCloudDataRepositoryInitializer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Seed(GeniViewCloudDataRepository context)
        {
            try
            {
                // Populate device event notification definitions
                foreach (var item in DeviceEventNotification.Seed())
                {
                    if (!context.DeviceEventActionNotifications.Any(x => x.UID == item.UID))
                        context.DeviceEventActionNotifications.Add(item);
                }

                // Populate application update entries
                foreach (var item in ApplicationUpdate.Seed())
                {
                    if (!context.ApplicationUpdates.Any(x => x.AppId == item.AppId))
                        context.ApplicationUpdates.Add(item);
                }

                // Create default agent for G3 flow
                var findAgent = context.Agents.FirstOrDefault(a => a.Name.ToLower() == "default");
                if (findAgent == null)
                {
                    var defaultAgent = Data.Agent.Agent.Default();
                    context.Agents.Add(defaultAgent);
                }

                // Sync PostgreSQL sequences with actual max IDs. Required after migrating data from
                // SQL Server: EnsureCreated creates sequences starting at 1, but imported rows
                // already occupy low IDs, causing PK_* duplicate-key violations on first INSERT.
                try
                {
                    context.Database.ExecuteSqlRaw(@"
DO $$
DECLARE
    tables text[] := ARRAY[
        '""AgentDeviceLogs""', '""InternalDeviceLogs""',
        '""AgentBatteryLogs""', '""InternalBatteryLogs""',
        '""DeviceEvents""', '""Devices""', '""Batteries""',
        '""Communities""', '""Groups""', '""Agents""'
    ];
    tbl text;
    seq text;
    maxid bigint;
BEGIN
    FOREACH tbl IN ARRAY tables LOOP
        BEGIN
            seq := pg_get_serial_sequence(tbl, 'ID');
            IF seq IS NOT NULL THEN
                EXECUTE format('SELECT COALESCE(MAX(""ID""), 0) FROM %s', tbl) INTO maxid;
                PERFORM setval(seq, maxid + 1, false);
            END IF;
        EXCEPTION WHEN OTHERS THEN
            NULL; -- table may not yet exist; skip silently
        END;
    END LOOP;
END $$;");
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Sequence sync skipped (non-fatal).");
                }

                // Create SQL views and functions (all idempotent via CREATE OR REPLACE)
                try
                {
                    context.Database.ExecuteSqlRaw(StoredProcedures.AgentBatteryLogsWithDurationView);
                    context.Database.ExecuteSqlRaw(StoredProcedures.AgentDeviceLogsWithDurationView);
                    context.Database.ExecuteSqlRaw(StoredProcedures.InternalBatteryLogsWithDurationView);
                    context.Database.ExecuteSqlRaw(StoredProcedures.InternalDeviceLogsWithDurationView);

                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetLatestBatteryCycleCount);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetLatestBatteryStateOfCharge);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetLatestBatteryTimestamp);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetLatestBatteryTemperature);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetLatestBatteryEfficiency);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetLatestBatteryStatus);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetBatteryActivityHistory);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetDeviceActivityHistory);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnPopupDashboard);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetDevicesList);
                    context.Database.ExecuteSqlRaw(StoredProcedures.FnGetBatteriesList);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Cannot create SQL views or functions.");
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Database seeding failed.");
            }
        }
    }
}
