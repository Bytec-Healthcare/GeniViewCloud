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
using System.Data.SqlClient;

namespace GeniView.Cloud.Repository
{
    public class DashboardDataRepository : IDisposable
    {
        private const string CycleStatusStoredProcedureName = "dbo.usp_GetLatestBatteryCycleCount";
        private const string StateOfChargeStoredProcedureName = "dbo.usp_GetLatestBatteryStateOfCharge";
        private const string EffectiveRotationStoredProcedureName = "dbo.usp_GetLatestBatteryTimestamp";
        private const string TemperatureStoredProcedureName = "dbo.usp_GetLatestBatteryTemperature";
        private const string BatteryEfficiencyStoredProcedureName = "dbo.usp_GetLatestBatteryEfficiency";
        private const string BatteryActivityHistoryStoredProcedureName = "dbo.usp_GetBatteryActivityHistory";
        private const string DeviceActivityHistoryStoredProcedureName = "dbo.usp_GetDeviceActivityHistory";
        private const string LatestBatteryStatusStoredProcedureName = "dbo.usp_GetLatestBatteryStatus";

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
            public double? Remaining_Capacity { get; set; }
            public int EventCode { get; set; }
        }

        private sealed class BatteryActivityHistoryRow
        {
            public DateTime ActivityDate { get; set; }
            public int TotalBatteriesInScope { get; set; }
            public int BatteriesOnline { get; set; }
            public int BatteriesOffline { get; set; }
        }

        private sealed class DeviceActivityHistoryRow
        {
            public DateTime ActivityDate { get; set; }
            public int TotalDevicesInScope { get; set; }
            public int DevicesOnline { get; set; }
            public int DevicesOffline { get; set; }
        }

        private sealed class LatestBatteryStatusRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public string DeviceSerialNumber { get; set; }
            public double? OperatingData_Current { get; set; }
            public string BatteryStatus { get; set; }
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

                decimal utilizedPowerSum = 0m; // In-use Remaining_Capacity sum
                decimal totalPowerSum = 0m;    // Total SlowChangingDataA_RemainingCapacity sum

                foreach (var row in rows)
                {
                    if (row == null)
                    {
                        continue;
                    }

                    // Total power in Amps = sum of SlowChangingDataA_RemainingCapacity for all batteries
                    if (row.SlowChangingDataA_RemainingCapacity.HasValue)
                    {
                        var totalCapDouble = row.SlowChangingDataA_RemainingCapacity.Value;
                        if (!double.IsNaN(totalCapDouble) && !double.IsInfinity(totalCapDouble))
                        {
                            totalPowerSum += (decimal)totalCapDouble;
                        }
                    }

                    // In-Use when EventCode == 18.
                    var isInUse = row.EventCode == 18;
                    if (!isInUse)
                    {
                        continue;
                    }

                    // Utilized power = sum of Remaining_Capacity for in-use batteries
                    if (!row.Remaining_Capacity.HasValue)
                    {
                        continue;
                    }

                    var utilizedCapDouble = row.Remaining_Capacity.Value;
                    if (double.IsNaN(utilizedCapDouble) || double.IsInfinity(utilizedCapDouble))
                    {
                        continue;
                    }

                    utilizedPowerSum += (decimal)utilizedCapDouble;
                }

                var efficiency = totalPowerSum > 0m
                    ? (int)Math.Round((utilizedPowerSum * 100m) / totalPowerSum, MidpointRounding.AwayFromZero)
                    : 0;

                if (efficiency < 0) efficiency = 0;
                if (efficiency > 100) efficiency = 100;

                var result = new BatteryEfficiencyModel
                {
                    PowerModulesCount = powerModulesCount,

                    // Widget fields:
                    // - total power in Amps  => totalPowerSum
                    // - utilized power       => utilizedPowerSum
                    TotalRemainingCapacitySum = totalPowerSum,
                    InUseRemainingCapacitySum = utilizedPowerSum,

                    EfficiencyScorePercent = efficiency,
                    InUsePercent = efficiency,
                    IdlePercent = 100m - efficiency
                };

                return result;
            }
        }

        public BatteryActivityHistoryModel GetBatteryActivityHistory(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = 180;

                // NOTE:
                // - requirement: use SP `usp_GetBatteryActivityHistory`.
                // - SP returns last 7 days activity rows.
                // - Scope filtering is typically handled inside the SP. If the SP accepts parameters,
                //   wire them in here (SqlParameter list) without changing the widget contract.
                var spRows = db.Database
                    .SqlQuery<BatteryActivityHistoryRow>("EXEC " + BatteryActivityHistoryStoredProcedureName)
                    .ToList();

                // Build 7 UTC calendar-day buckets ending today (UTC).
                var endDayUtc = DateTime.UtcNow.Date;
                var startDayUtc = endDayUtc.AddDays(-6);

                // Map by UTC calendar date.
                // Map by calendar date (do not shift SQL DATE/datetime buckets via ToUniversalTime()).
                var byDay = spRows
                    .Where(r => r != null)
                    .GroupBy(r => r.ActivityDate.Date)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.ActivityDate).First());

                var days = new List<BatteryActivityHistoryDay>(7);

                for (var i = 0; i < 7; i++)
                {
                    var day = startDayUtc.AddDays(i);

                    BatteryActivityHistoryRow row;
                    byDay.TryGetValue(day, out row);

                    days.Add(new BatteryActivityHistoryDay
                    {
                        ActivityDateUtc = day,
                        TotalBatteriesInScope = row != null ? row.TotalBatteriesInScope : 0,
                        BatteriesOnline = row != null ? row.BatteriesOnline : 0,
                        BatteriesOffline = row != null ? row.BatteriesOffline : 0,
                        Label = day.ToString("dd/MM/yyyy")
                    });
                }

                return new BatteryActivityHistoryModel
                {
                    Days = days
                };
            }
        }

        public DeviceActivityHistoryModel GetDeviceActivityHistory(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = 180;

                // NOTE:
                // - requirement: use SP `usp_GetDeviceActivityHistory`.
                // - SP returns last 7 days activity rows.
                // - Scope filtering is typically handled inside the SP. If the SP accepts parameters,
                //   wire them in here (SqlParameter list) without changing the widget contract.
                var spRows = db.Database
                    .SqlQuery<DeviceActivityHistoryRow>("EXEC " + DeviceActivityHistoryStoredProcedureName)
                    .ToList();

                // Build 7 UTC calendar-day buckets ending today (UTC).
                var endDayUtc = DateTime.UtcNow.Date;
                var startDayUtc = endDayUtc.AddDays(-6);

                // Map by UTC calendar date.
                // Map by calendar date (do not shift SQL DATE/datetime buckets via ToUniversalTime()).
var byDay = spRows
    .Where(r => r != null)
    .GroupBy(r => r.ActivityDate.Date)
    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.ActivityDate).First());

                var days = new List<DeviceActivityHistoryDay>(7);

                for (var i = 0; i < 7; i++)
                {
                    var day = startDayUtc.AddDays(i);

                    DeviceActivityHistoryRow row;
                    byDay.TryGetValue(day, out row);

                    days.Add(new DeviceActivityHistoryDay
                    {
                        ActivityDateUtc = day,
                        TotalDevicesInScope = row != null ? row.TotalDevicesInScope : 0,
                        DevicesOnline = row != null ? row.DevicesOnline : 0,
                        DevicesOffline = row != null ? row.DevicesOffline : 0,
                        Label = day.ToString("dd/MM/yyyy")
                    });
                }

                return new DeviceActivityHistoryModel
                {
                    Days = days
                };
            }
        }

        private static string NormalizeStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // Normalize unicode dashes to hyphen and collapse whitespace.
            var v = value
                .Replace('\u2013', '-')  // en dash
                .Replace('\u2014', '-')  // em dash
                .Replace('\u2212', '-')  // minus
                .Replace('–', '-')
                .Replace('—', '-')
                .Trim();

            // Collapse multiple spaces.
            while (v.Contains("  "))
            {
                v = v.Replace("  ", " ");
            }

            return v;
        }

        private static bool ContainsToken(string haystack, string token)
        {
            if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(token)) return false;
            return haystack.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public BatteryStatusModel GetBatteryStatus(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = 180;

                var rows = db.Database
                    .SqlQuery<LatestBatteryStatusRow>("EXEC " + LatestBatteryStatusStoredProcedureName)
                    .ToList();

                var onDeviceCharging = 0;
                var onDeviceDischarging = 0;
                var onDeviceIdle = 0;
                var offDeviceCharging = 0;
                var offDeviceIdle = 0;

                foreach (var row in rows)
                {
                    var status = NormalizeStatus(row != null ? row.BatteryStatus : null);

                    // Important: handle SP values like:
                    // "On Device – Discharging" (en dash) or "On Device - Discharging" (hyphen)
                    // and potential extra spaces.
                    var isOn = ContainsToken(status, "On Device");
                    var isOff = ContainsToken(status, "Off Device");

                    if (isOn)
                    {
                        if (ContainsToken(status, "Discharging")) onDeviceDischarging++;
                        else if (ContainsToken(status, "Charging")) onDeviceCharging++;
                        else if (ContainsToken(status, "Idle")) onDeviceIdle++;
                        else onDeviceIdle++;
                    }
                    else if (isOff)
                    {
                        if (ContainsToken(status, "Charging")) offDeviceCharging++;
                        else if (ContainsToken(status, "Idle")) offDeviceIdle++;
                        else offDeviceIdle++;
                    }
                    else
                    {
                        // If SP gives an unexpected value, bucket it into Off Device Idle so counts still match total.
                        offDeviceIdle++;
                    }
                }

                var total = rows.Count;
                var onTotal = onDeviceCharging + onDeviceDischarging + onDeviceIdle;
                var offTotal = offDeviceCharging + offDeviceIdle;

                // Ensure totals always match row count even if a row comes back empty/unexpected.
                var drift = total - (onTotal + offTotal);
                if (drift != 0)
                {
                    // Apply drift to Off Device Idle to keep UI consistent.
                    offDeviceIdle += drift;
                    if (offDeviceIdle < 0) offDeviceIdle = 0;

                    offTotal = offDeviceCharging + offDeviceIdle;
                }

                var efficiency = total > 0
                    ? (int)Math.Round(((decimal)onTotal * 100m) / total, MidpointRounding.AwayFromZero)
                    : 0;

                if (efficiency < 0) efficiency = 0;
                if (efficiency > 100) efficiency = 100;

                decimal p1 = 0m, p2 = 0m, p3 = 0m, p4 = 0m, p5 = 0m;
                if (total > 0)
                {
                    p1 = Math.Round((decimal)onDeviceCharging * 100m / total, 2);
                    p2 = Math.Round((decimal)onDeviceDischarging * 100m / total, 2);
                    p3 = Math.Round((decimal)onDeviceIdle * 100m / total, 2);
                    p4 = Math.Round((decimal)offDeviceCharging * 100m / total, 2);
                    p5 = Math.Round((decimal)offDeviceIdle * 100m / total, 2);

                    // Fix rounding drift to exactly 100%.
                    var sum = p1 + p2 + p3 + p4 + p5;
                    if (sum != 100m)
                    {
                        p5 = 100m - (p1 + p2 + p3 + p4);
                        if (p5 < 0m) p5 = 0m;
                    }
                }

                return new BatteryStatusModel
                {
                    PowerModulesCount = total,

                    OnDeviceChargingCount = onDeviceCharging,
                    OnDeviceDischargingCount = onDeviceDischarging,
                    OnDeviceIdleCount = onDeviceIdle,

                    OffDeviceChargingCount = offDeviceCharging,
                    OffDeviceIdleCount = offDeviceIdle,

                    OnDeviceTotalCount = onTotal,
                    OffDeviceTotalCount = offTotal,

                    EfficiencyScorePercent = efficiency,

                    OnDeviceChargingPercent = p1,
                    OnDeviceDischargingPercent = p2,
                    OnDeviceIdlePercent = p3,
                    OffDeviceChargingPercent = p4,
                    OffDeviceIdlePercent = p5
                };
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