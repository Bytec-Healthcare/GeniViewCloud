using GeniView.Cloud.Models;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Globalization;
using GeniView.Data.Hardware.Event;
using GeniView.Cloud.Common;

namespace GeniView.Cloud.Repository
{
    public class DashboardDataRepository : IDisposable
    {
        private const string CycleStatusStoredProcedureName = "dbo.usp_GetLatestBatteryCycleCount";
        private const string StateOfChargeStoredProcedureName = "dbo.usp_GetLatestBatteryStateOfCharge";
        private const string EffectiveRotationStoredProcedureName = "dbo.usp_GetLatestBatteryTimestamp";
        private const string TemperatureStoredProcedureName = "dbo.usp_GetLatestBatteryTemperature";
        private const string BatteryEfficiencyStoredProcedureName = "dbo.usp_GetLatestBatteryEfficiency";

        private sealed class CycleStatusRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public int? OperatingData_CycleCount { get; set; }
        }

        private sealed class StateOfChargeRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public int SlowChangingDataA_RelativeStateOfCharge { get; set; }
        }

        private sealed class EffectiveRotationRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private sealed class TemperatureRow
        {
            public long Battery_ID { get; set; }

            public int SlowChangingDataB_BatteryInternalTemperature { get; set; }

            public int EventCode { get; set; }
        }

        private sealed class BatteryEfficiencyRow
        {
            public long Battery_ID { get; set; }
            public double? SlowChangingDataA_RemainingCapacity { get; set; }
            public string DeviceSerialNumber { get; set; }
            public int EventCode { get; set; }
        }

        private Random randomDouble = new Random();

        #region Dashboard

        public CycleStatusModel GetCycleStatus(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = 180;
                // SP already returns latest row per Battery_ID.
                var spRows = db.Database.SqlQuery<CycleStatusRow>("EXEC " + CycleStatusStoredProcedureName)
                    .ToList();

                // Battery entity doesn't expose FK properties, so read IDs via navigation properties.
                // Build a candidate list of batteries, but if it results in 0 batteries (common when
                // batteries are not assigned to a community/group), fall back to SP rows.
                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = (long?)b.Community.ID,
                        GroupID = (long?)b.Group.ID
                    })
                    .ToList();

                if (communityID != null)
                {
                    batteryQuery = batteryQuery.Where(x => x.CommunityID == communityID).ToList();
                }

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    var includedGroupIds = allChildrenGroups.Select(g => g.ID).ToHashSet();
                    includedGroupIds.Add(groupID.Value);

                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID != null && includedGroupIds.Contains(x.GroupID.Value))
                        .ToList();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    batteryQuery = batteryQuery.Where(x => x.GroupID == groupID).ToList();
                }

                var allowedBatteryIds = batteryQuery.Select(x => x.BatteryID).ToHashSet();

                // If the filter produced no batteries, don't zero the widget; show at least what the SP returns.
                var rows = allowedBatteryIds.Count > 0
                    ? spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList()
                    : spRows;

                var low = 0;
                var high = 0;
                var eol = 0;

                var validCycleSum = 0;
                var validCycleCount = 0;

                foreach (var row in rows)
                {
                    // Average Cycle Count: include only numeric values (NULLs excluded by nullable mapping).
                    if (row.OperatingData_CycleCount.HasValue)
                    {
                        validCycleSum += row.OperatingData_CycleCount.Value;
                        validCycleCount++;
                    }

                    // Keep bucketing behavior; skip invalid cycle counts so they don't get mis-bucketed.
                    if (!row.OperatingData_CycleCount.HasValue)
                    {
                        continue;
                    }

                    var cycleCount = row.OperatingData_CycleCount.Value;

                    // Keep the existing thresholds but map them to the UX wording.
                    if (cycleCount < 500)
                    {
                        low++;
                    }
                    else if (cycleCount <= 1000)
                    {
                        high++;
                    }
                    else
                    {
                        eol++;
                    }
                }

                var total = low + high + eol;

                var result = new CycleStatusModel
                {
                    LowCount = low,
                    HighCount = high,
                    EndOfLifeCount = eol,
                    TotalCount = total,
                    PowerModulesCount = allowedBatteryIds.Count > 0 ? allowedBatteryIds.Count : rows.Count,

                    // UI currently uses int: round to nearest whole number.
                    AverageCycleCount = validCycleCount > 0
                        ? (int)Math.Round((decimal)validCycleSum / validCycleCount, MidpointRounding.AwayFromZero)
                        : 0
                };

                if (total > 0)
                {
                    result.LowPercent = Math.Round((decimal)low * 100m / total, 2);
                    result.HighPercent = Math.Round((decimal)high * 100m / total, 2);
                    result.EndOfLifePercent = Math.Round((decimal)eol * 100m / total, 2);
                }

                return result;
            }
        }

        public StateOfChargeModel GetStateOfCharge(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = 180;
                // SP already returns latest row per Battery_ID.
                var spRows = db.Database.SqlQuery<StateOfChargeRow>("EXEC " + StateOfChargeStoredProcedureName)
                    .ToList();

                // Battery entity doesn't expose FK properties, so read IDs via navigation properties.
                // Build a candidate list of batteries, but if it results in 0 batteries (common when
                // batteries are not assigned to a community/group), fall back to SP rows.
                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = (long?)b.Community.ID,
                        GroupID = (long?)b.Group.ID
                    })
                    .ToList();

                if (communityID != null)
                {
                    batteryQuery = batteryQuery.Where(x => x.CommunityID == communityID).ToList();
                }

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    var includedGroupIds = allChildrenGroups.Select(g => g.ID).ToHashSet();
                    includedGroupIds.Add(groupID.Value);

                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID != null && includedGroupIds.Contains(x.GroupID.Value))
                        .ToList();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    batteryQuery = batteryQuery.Where(x => x.GroupID == groupID).ToList();
                }

                var allowedBatteryIds = batteryQuery.Select(x => x.BatteryID).ToHashSet();

                var rows = allowedBatteryIds.Count > 0
                    ? spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList()
                    : spRows;

                var high = 0;
                var low = 0;
                var chargeNow = 0;
                var validSocSum = 0;
                var validSocCount = 0;

                foreach (var row in rows)
                {
                    var soc = row.SlowChangingDataA_RelativeStateOfCharge;

                    // Average SoC: exclude invalid values (and NULL if ever represented).
                    if (soc >= 0 && soc <= 100)
                    {
                        validSocSum += soc;
                        validSocCount++;
                    }

                    if (soc > 70)
                    {
                        high++;
                    }
                    else if (soc >= 30)
                    {
                        low++;
                    }
                    else
                    {
                        chargeNow++;
                    }
                }

                var total = high + low + chargeNow;

                var result = new StateOfChargeModel
                {
                    HighSoCCount = high,
                    LowSoCCount = low,
                    ChargeNowCount = chargeNow,
                    TotalCount = total,
                    PowerModulesCount = allowedBatteryIds.Count > 0 ? allowedBatteryIds.Count : rows.Count,
                    AverageSoC = validSocCount > 0
                        ? (int)Math.Round((decimal)validSocSum / validSocCount, MidpointRounding.AwayFromZero)
                        : 0
                };

                if (total > 0)
                {
                    result.HighSoCPercent = Math.Round((decimal)high * 100m / total, 2);
                    result.LowSoCPercent = Math.Round((decimal)low * 100m / total, 2);
                    result.ChargeNowPercent = Math.Round((decimal)chargeNow * 100m / total, 2);
                }

                return result;
            }
        }

        public EffectiveRotationModel GetEffectiveRotation(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = 180;

                // SP already returns latest row per Battery_ID.
                var spRows = db.Database.SqlQuery<EffectiveRotationRow>("EXEC " + EffectiveRotationStoredProcedureName)
                    .ToList();

                // Battery entity doesn't expose FK properties, so read IDs via navigation properties.
                // Build a candidate list of batteries, but if it results in 0 batteries (common when
                // batteries are not assigned to a community/group), fall back to SP rows.
                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = (long?)b.Community.ID,
                        GroupID = (long?)b.Group.ID
                    })
                    .ToList();

                if (communityID != null)
                {
                    batteryQuery = batteryQuery.Where(x => x.CommunityID == communityID).ToList();
                }

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    var includedGroupIds = allChildrenGroups.Select(g => g.ID).ToHashSet();
                    includedGroupIds.Add(groupID.Value);

                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID != null && includedGroupIds.Contains(x.GroupID.Value))
                        .ToList();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    batteryQuery = batteryQuery.Where(x => x.GroupID == groupID).ToList();
                }

                var allowedBatteryIds = batteryQuery.Select(x => x.BatteryID).ToHashSet();

                var rows = allowedBatteryIds.Count > 0
                    ? spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList()
                    : spRows;

                var nowUtc = DateTime.UtcNow;

                var good = 0;
                var average = 0;
                var poor = 0;

                foreach (var row in rows)
                {
                    var rowUtc = row.Timestamp.Kind == DateTimeKind.Utc
                        ? row.Timestamp
                        : row.Timestamp.ToUniversalTime();

                    var days = (nowUtc - rowUtc).TotalDays;

                    if (days < 5d)
                    {
                        good++;
                    }
                    else if (days <= 10d)
                    {
                        average++;
                    }
                    else
                    {
                        poor++;
                    }
                }

                // Total buckets from rows; scope denominator must be total batteries in scope (PowerModulesCount).
                var bucketTotal = good + average + poor;
                var powerModulesCount = allowedBatteryIds.Count > 0 ? allowedBatteryIds.Count : rows.Count;

                var result = new EffectiveRotationModel
                {
                    GoodCount = good,
                    AverageCount = average,
                    PoorCount = poor,
                    TotalCount = bucketTotal,
                    PowerModulesCount = powerModulesCount,
                    EfficiencyScorePercent = powerModulesCount > 0
                        ? (int)Math.Round(((decimal)good * 100m) / powerModulesCount, MidpointRounding.AwayFromZero)
                        : 0
                };

                if (bucketTotal > 0)
                {
                    result.GoodPercent = Math.Round((decimal)good * 100m / bucketTotal, 2);
                    result.AveragePercent = Math.Round((decimal)average * 100m / bucketTotal, 2);
                    result.PoorPercent = Math.Round((decimal)poor * 100m / bucketTotal, 2);
                }

                return result;
            }
        }

        public TemperatureModel GetTemperature(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = 180;

                // SP returns: Battery_ID, SlowChangingDataB_BatteryInternalTemperature, EventCode
                // Per schema: temperature is int NOT NULL; EventCode is int NOT NULL.
                var rows = db.Database.SqlQuery<TemperatureRow>("EXEC " + TemperatureStoredProcedureName)
                    .ToList();

                var powerModulesCount = rows.Count;

                var chargingNormal = 0;
                var chargingWarning = 0;
                var dischargingNormal = 0;
                var dischargingWarning = 0;

                var validTempCount = 0;
                var totalNormalCount = 0;

                foreach (var row in rows)
                {
                    if (row == null)
                    {
                        continue;
                    }

                    // 1) Derive state FIRST from EventCode (3/4 only).
                    //var isCharging = row.EventCode == 3;
                    //var isDischargingOrIdle = row.EventCode == 4;

                    //if (!isCharging && !isDischargingOrIdle)
                    //{
                    //    continue;
                    //}


                    var isCharging = row.EventCode == 3;
                    var isDischargingOrIdle = !isCharging;

                    // 2) Temperature is always present per schema.
                    var temp = (double)row.SlowChangingDataB_BatteryInternalTemperature;

                    validTempCount++;

                    // 3) Thresholding AFTER state derivation.
                    if (isCharging)
                    {
                        if (temp <= 35d)
                        {
                            chargingNormal++;
                            totalNormalCount++;
                        }
                        else
                        {
                            chargingWarning++;
                        }
                    }
                    else
                    {
                        if (temp <= 30d)
                        {
                            dischargingNormal++;
                            totalNormalCount++;
                        }
                        else
                        {
                            dischargingWarning++;
                        }
                    }
                }

                var totalWarningCount = chargingWarning + dischargingWarning;

                var result = new TemperatureModel
                {
                    PowerModulesCount = powerModulesCount,
                    ChargingNormalCount = chargingNormal,
                    ChargingWarningCount = chargingWarning,
                    DischargingNormalCount = dischargingNormal,
                    DischargingWarningCount = dischargingWarning,
                    TotalValidTempCount = validTempCount,
                    EfficiencyScorePercent = validTempCount > 0
                        ? (int)Math.Round(((decimal)totalNormalCount * 100m) / validTempCount, MidpointRounding.AwayFromZero)
                        : 0
                };

                if (validTempCount > 0)
                {
                    result.NormalPercent = Math.Round(((decimal)totalNormalCount * 100m) / validTempCount, 2);
                    result.WarningPercent = Math.Round(((decimal)totalWarningCount * 100m) / validTempCount, 2);
                }

                return result;
            }
        }

        public BatteryEfficiencyModel GetBatteryEfficiency(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = 180;

                // NOTE: Per requirement: use ONLY the SP output; no additional filtering operations.
                var rows = db.Database.SqlQuery<BatteryEfficiencyRow>("EXEC " + BatteryEfficiencyStoredProcedureName)
                    .ToList();

                var powerModulesCount = rows
                    .Where(r => r != null)
                    .Select(r => r.Battery_ID)
                    .Distinct()
                    .Count();

                decimal inUseSum = 0m;
                decimal totalSum = 0m;

                foreach (var row in rows)
                {
                    if (row == null)
                    {
                        continue;
                    }

                    if (!row.SlowChangingDataA_RemainingCapacity.HasValue)
                    {
                        continue;
                    }

                    // float in DB materializes as double in .NET
                    var remainingCapacityDouble = row.SlowChangingDataA_RemainingCapacity.Value;
                    if (double.IsNaN(remainingCapacityDouble) || double.IsInfinity(remainingCapacityDouble))
                    {
                        continue;
                    }

                    var remainingCapacity = (decimal)remainingCapacityDouble;

                    totalSum += remainingCapacity;

                    // In-Use when EventCode == 18 OR DeviceSerialNumber NOT NULL.
                    // Idle when EventCode == 19 OR DeviceSerialNumber NULL (or no 18/19 and DeviceSerialNumber NULL).
                    var isInUse = row.EventCode == 18 || !string.IsNullOrWhiteSpace(row.DeviceSerialNumber);

                    if (isInUse)
                    {
                        inUseSum += remainingCapacity;
                    }
                }

                var efficiency = totalSum > 0m
                    ? (int)Math.Round((inUseSum * 100m) / totalSum, MidpointRounding.AwayFromZero)
                    : 0;

                if (efficiency < 0) efficiency = 0;
                if (efficiency > 100) efficiency = 100;

                var result = new BatteryEfficiencyModel
                {
                    PowerModulesCount = powerModulesCount,
                    InUseRemainingCapacitySum = inUseSum,
                    TotalRemainingCapacitySum = totalSum,
                    EfficiencyScorePercent = efficiency,
                    InUsePercent = efficiency,
                    IdlePercent = 100m - efficiency
                };

                return result;
            }
        }

        #endregion

        #region AdminDashboard
        public AdminDashboardModel GetAdminDashboard()
        {
            AdminDashboardModel model = new AdminDashboardModel();
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                model.Communities = db.Communities.Count();
                model.Groups = db.Groups.Count();
                using (var userdb = new ApplicationDbContext())
                {
                    model.Users = userdb.Users.Count();
                }
                model.RegDevices = db.Devices.Where(x => x.Community != null).Count();
                model.RemDevices = db.Devices.Where(x => x.Community == null).Count();

                model.RegBatteries = db.Batteries.Where(x => x.Community != null).Count();
                model.RemBatteries = db.Batteries.Where(x => x.Community == null).Count();
                return model;
            }
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}