using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class DashboardDataRepository : IDisposable
    {
        // ── Private row types for SqlQueryRaw projections ─────────────────────

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

        // ─────────────────────────────────────────────────────────────────────

        private readonly GeniViewCloudDataRepository _db;

        public DashboardDataRepository(GeniViewCloudDataRepository db)
        {
            _db = db;
        }

        // ── Battery scope helper (shared by all widget methods) ───────────────

        private HashSet<long> GetAllowedBatteryIds(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            var query = _db.Batteries.AsNoTracking()
                .Where(b => !b.IsDeactivated)
                .Select(b => new { BatteryID = b.ID, CommunityID = b.CommunityID, GroupID = b.GroupID })
                .ToList();

            if (communityID != null)
                query = query.Where(x => x.CommunityID == communityID).ToList();

            if (communityID != null && groupID != null && includeAllSubGroups)
            {
                var allGroupIds = new GroupsDataRepository(_db)
                    .GetGroups(communityID, groupID)
                    .Select(g => g.ID).ToHashSet();
                allGroupIds.Add(groupID.Value);
                query = query.Where(x => x.GroupID != null && allGroupIds.Contains(x.GroupID.Value)).ToList();
            }
            else if (groupID != null)
            {
                query = query.Where(x => x.GroupID == groupID).ToList();
            }

            return query.Select(x => x.BatteryID).ToHashSet();
        }

        // ── NpgsqlParameter helpers for nullable parameters ───────────────────

        private static NpgsqlParameter LongParam(string name, long? value) =>
            new NpgsqlParameter(name, value.HasValue ? (object)value.Value : DBNull.Value);

        private static NpgsqlParameter BoolParam(string name, bool value) =>
            new NpgsqlParameter(name, value);

        #region Dashboard

        public CycleStatusModel GetCycleStatus(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            _db.Database.SetCommandTimeout(500);

            var spRows = _db.Database
                .SqlQueryRaw<CycleStatusRow>("""SELECT * FROM usp_GetLatestBatteryCycleCount()""")
                .ToList();

            var allowedBatteryIds = GetAllowedBatteryIds(communityID, groupID, includeAllSubGroups);
            var rows = spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList();

            if (allowedBatteryIds.Count == 0 || rows.Count == 0)
            {
                return new CycleStatusModel
                {
                    LowCount = 0, HighCount = 0, EndOfLifeCount = 0, TotalCount = 0,
                    PowerModulesCount = allowedBatteryIds.Count,
                    AverageCycleCount = 0, LowPercent = 0, HighPercent = 0, EndOfLifePercent = 0
                };
            }

            var low = 0; var high = 0; var eol = 0;
            var lowIds = new List<long>(); var highIds = new List<long>(); var eolIds = new List<long>();
            var validSum = 0; var validCount = 0;

            foreach (var row in rows)
            {
                if (!row.OperatingData_CycleCount.HasValue) continue;
                validSum += row.OperatingData_CycleCount.Value;
                validCount++;
                var cc = row.OperatingData_CycleCount.Value;
                if (cc < 500) { low++; lowIds.Add(row.Battery_ID); }
                else if (cc <= 1000) { high++; highIds.Add(row.Battery_ID); }
                else { eol++; eolIds.Add(row.Battery_ID); }
            }

            var total = low + high + eol;
            var result = new CycleStatusModel
            {
                LowCount = low, HighCount = high, EndOfLifeCount = eol, TotalCount = total,
                PowerModulesCount = allowedBatteryIds.Count,
                AverageCycleCount = validCount > 0
                    ? (int)Math.Round((decimal)validSum / validCount, MidpointRounding.AwayFromZero) : 0,
                LowBatteryIds = lowIds, HighBatteryIds = highIds, EndOfLifeBatteryIds = eolIds
            };
            if (total > 0)
            {
                result.LowPercent = Math.Round((decimal)low * 100m / total, 2);
                result.HighPercent = Math.Round((decimal)high * 100m / total, 2);
                result.EndOfLifePercent = Math.Round((decimal)eol * 100m / total, 2);
            }
            return result;
        }


        public StateOfChargeModel GetStateOfCharge(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            _db.Database.SetCommandTimeout(500);

            var spRows = _db.Database
                .SqlQueryRaw<StateOfChargeRow>("""SELECT * FROM usp_GetLatestBatteryStateOfCharge()""")
                .ToList();

            var allowedBatteryIds = GetAllowedBatteryIds(communityID, groupID, includeAllSubGroups);
            var rows = spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList();

            int high = 0; int low = 0; int chargeNow = 0;
            int validSum = 0; int validCount = 0;
            var highIds = new List<long>(); var lowIds = new List<long>(); var chargeNowIds = new List<long>();

            foreach (var row in rows)
            {
                int soc = row.SlowChangingDataA_RelativeStateOfCharge;
                if (soc >= 0 && soc <= 100) { validSum += soc; validCount++; }
                if (soc > 70) { high++; highIds.Add(row.Battery_ID); }
                else if (soc >= 30) { low++; lowIds.Add(row.Battery_ID); }
                else { chargeNow++; chargeNowIds.Add(row.Battery_ID); }
            }

            int total = high + low + chargeNow;
            var result = new StateOfChargeModel
            {
                HighSoCCount = high, LowSoCCount = low, ChargeNowCount = chargeNow, TotalCount = total,
                PowerModulesCount = allowedBatteryIds.Count,
                AverageSoC = validCount > 0
                    ? (int)Math.Round((decimal)validSum / validCount, MidpointRounding.AwayFromZero) : 0,
                HighSoCBatteryIds = highIds, LowSoCBatteryIds = lowIds, ChargeNowBatteryIds = chargeNowIds
            };
            if (total > 0)
            {
                result.HighSoCPercent = Math.Round((decimal)high * 100m / total, 2);
                result.LowSoCPercent = Math.Round((decimal)low * 100m / total, 2);
                result.ChargeNowPercent = Math.Round((decimal)chargeNow * 100m / total, 2);
            }
            else { result.HighSoCPercent = 0; result.LowSoCPercent = 0; result.ChargeNowPercent = 0; }
            return result;
        }


        public EffectiveRotationModel GetEffectiveRotation(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            _db.Database.SetCommandTimeout(500);

            var spRows = _db.Database
                .SqlQueryRaw<EffectiveRotationRow>("""SELECT * FROM usp_GetLatestBatteryTimestamp()""")
                .ToList();

            var allowedBatteryIds = GetAllowedBatteryIds(communityID, groupID, includeAllSubGroups);

            if (allowedBatteryIds.Count == 0)
                return new EffectiveRotationModel
                {
                    GoodCount = 0, AverageCount = 0, PoorCount = 0, TotalCount = 0,
                    PowerModulesCount = 0, EfficiencyScorePercent = 0,
                    GoodPercent = 0, AveragePercent = 0, PoorPercent = 0
                };

            var rows = spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList();
            var nowUtc = DateTime.UtcNow;
            var good = 0; var average = 0; var poor = 0;
            var goodIds = new List<long>(); var averageIds = new List<long>(); var poorIds = new List<long>();

            foreach (var row in rows)
            {
                var rowUtc = row.Timestamp.Kind == DateTimeKind.Utc ? row.Timestamp : row.Timestamp.ToUniversalTime();
                var days = (nowUtc - rowUtc).TotalDays;
                if (days < 5d) { good++; goodIds.Add(row.Battery_ID); }
                else if (days <= 10d) { average++; averageIds.Add(row.Battery_ID); }
                else { poor++; poorIds.Add(row.Battery_ID); }
            }

            var total = good + average + poor;
            var pmCount = allowedBatteryIds.Count;
            var result = new EffectiveRotationModel
            {
                GoodCount = good, AverageCount = average, PoorCount = poor, TotalCount = total,
                PowerModulesCount = pmCount,
                EfficiencyScorePercent = pmCount > 0
                    ? (int)Math.Round(((decimal)good * 100m) / pmCount, MidpointRounding.AwayFromZero) : 0,
                GoodBatteryIds = goodIds, AverageBatteryIds = averageIds, PoorBatteryIds = poorIds
            };
            if (total > 0)
            {
                result.GoodPercent = Math.Round((decimal)good * 100m / total, 2);
                result.AveragePercent = Math.Round((decimal)average * 100m / total, 2);
                result.PoorPercent = Math.Round((decimal)poor * 100m / total, 2);
            }
            return result;
        }


        public TemperatureModel GetTemperature(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            _db.Database.SetCommandTimeout(500);

            var spRows = _db.Database
                .SqlQueryRaw<TemperatureRow>("""SELECT * FROM usp_GetLatestBatteryTemperature()""")
                .ToList();

            var allowedBatteryIds = GetAllowedBatteryIds(communityID, groupID, includeAllSubGroups);

            if (allowedBatteryIds.Count == 0)
                return new TemperatureModel
                {
                    PowerModulesCount = 0, ChargingNormalCount = 0, ChargingWarningCount = 0,
                    DischargingNormalCount = 0, DischargingWarningCount = 0,
                    TotalValidTempCount = 0, EfficiencyScorePercent = 0, NormalPercent = 0, WarningPercent = 0
                };

            var rows = spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList();
            var chargingNormal = 0; var chargingWarning = 0;
            var dischargingNormal = 0; var dischargingWarning = 0;
            var validTempCount = 0; var totalNormalCount = 0;
            var cnIds = new List<long>(); var cwIds = new List<long>();
            var dnIds = new List<long>(); var dwIds = new List<long>();

            foreach (var row in rows)
            {
                var isCharging = row.EventCode == 3;
                var temp = (double)row.SlowChangingDataB_BatteryInternalTemperature;
                validTempCount++;
                if (isCharging)
                {
                    if (temp <= 35) { chargingNormal++; totalNormalCount++; cnIds.Add(row.Battery_ID); }
                    else { chargingWarning++; cwIds.Add(row.Battery_ID); }
                }
                else
                {
                    if (temp <= 30) { dischargingNormal++; totalNormalCount++; dnIds.Add(row.Battery_ID); }
                    else { dischargingWarning++; dwIds.Add(row.Battery_ID); }
                }
            }

            var result = new TemperatureModel
            {
                PowerModulesCount = allowedBatteryIds.Count,
                ChargingNormalCount = chargingNormal, ChargingWarningCount = chargingWarning,
                DischargingNormalCount = dischargingNormal, DischargingWarningCount = dischargingWarning,
                TotalValidTempCount = validTempCount,
                EfficiencyScorePercent = validTempCount > 0
                    ? (int)Math.Round((decimal)totalNormalCount * 100m / validTempCount, MidpointRounding.AwayFromZero) : 0,
                ChargingNormalBatteryIds = cnIds, ChargingWarningBatteryIds = cwIds,
                DischargingNormalBatteryIds = dnIds, DischargingWarningBatteryIds = dwIds
            };
            if (validTempCount > 0)
            {
                result.NormalPercent = Math.Round((decimal)totalNormalCount * 100m / validTempCount, 2);
                result.WarningPercent = Math.Round((decimal)(chargingWarning + dischargingWarning) * 100m / validTempCount, 2);
            }
            return result;
        }


        public BatteryEfficiencyModel GetBatteryEfficiency(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            _db.Database.SetCommandTimeout(500);

            var spRows = _db.Database
                .SqlQueryRaw<BatteryEfficiencyRow>("""SELECT * FROM usp_GetLatestBatteryEfficiency()""")
                .ToList();

            var allowedBatteryIds = GetAllowedBatteryIds(communityID, groupID, includeAllSubGroups);

            if (allowedBatteryIds.Count == 0)
                return new BatteryEfficiencyModel
                {
                    PowerModulesCount = 0, TotalRemainingCapacitySum = 0m,
                    InUseRemainingCapacitySum = 0m, EfficiencyScorePercent = 0,
                    InUsePercent = 0m, IdlePercent = 0m
                };

            var rows = spRows.Where(r => r != null && allowedBatteryIds.Contains(r.Battery_ID)).ToList();
            decimal utilized = 0m; decimal totalCap = 0m;

            foreach (var row in rows)
            {
                if (row.SlowChangingDataA_RemainingCapacity.HasValue)
                {
                    var cap = row.SlowChangingDataA_RemainingCapacity.Value;
                    if (!double.IsNaN(cap) && !double.IsInfinity(cap)) totalCap += (decimal)cap;
                }
                if (row.EventCode != 18 || !row.Remaining_Capacity.HasValue) continue;
                var used = row.Remaining_Capacity.Value;
                if (!double.IsNaN(used) && !double.IsInfinity(used)) utilized += (decimal)used;
            }

            var efficiency = totalCap > 0m
                ? (int)Math.Round((utilized * 100m) / totalCap, MidpointRounding.AwayFromZero) : 0;
            efficiency = Math.Max(0, Math.Min(100, efficiency));

            return new BatteryEfficiencyModel
            {
                PowerModulesCount = allowedBatteryIds.Count,
                TotalRemainingCapacitySum = totalCap,
                InUseRemainingCapacitySum = utilized,
                EfficiencyScorePercent = efficiency,
                InUsePercent = efficiency,
                IdlePercent = 100m - efficiency
            };
        }


        public BatteryActivityHistoryModel GetBatteryActivityHistory(
            long? communityID, long? groupID, bool includeAllSubGroups)
        {
            _db.Database.SetCommandTimeout(500);

            var spRows = _db.Database
                .SqlQueryRaw<BatteryActivityHistoryRow>(
                    """SELECT * FROM usp_GetBatteryActivityHistory(@cid, @gid, @sub)""",
                    LongParam("cid", communityID),
                    LongParam("gid", groupID),
                    BoolParam("sub", includeAllSubGroups))
                .ToList();

            var endDayUtc = DateTime.UtcNow.Date;
            var startDayUtc = endDayUtc.AddDays(-6);
            var byDay = spRows.Where(r => r != null)
                .GroupBy(r => r.ActivityDate.Date)
                .ToDictionary(g => g.Key, g => g.First());

            var days = new List<BatteryActivityHistoryDay>(7);
            for (int i = 0; i < 7; i++)
            {
                var day = startDayUtc.AddDays(i);
                byDay.TryGetValue(day, out var row);
                days.Add(new BatteryActivityHistoryDay
                {
                    ActivityDateUtc = day,
                    TotalBatteriesInScope = row?.TotalBatteriesInScope ?? 0,
                    BatteriesOnline = row?.BatteriesOnline ?? 0,
                    BatteriesOffline = row?.BatteriesOffline ?? 0,
                    Label = day.ToString("dd/MM/yyyy")
                });
            }
            return new BatteryActivityHistoryModel { Days = days };
        }


        public DeviceActivityHistoryModel GetDeviceActivityHistory(
            long? communityID, long? groupID, bool includeAllSubGroups)
        {
            _db.Database.SetCommandTimeout(500);

            var spRows = _db.Database
                .SqlQueryRaw<DeviceActivityHistoryRow>(
                    """SELECT * FROM usp_GetDeviceActivityHistory(@cid, @gid, @sub)""",
                    LongParam("cid", communityID),
                    LongParam("gid", groupID),
                    BoolParam("sub", includeAllSubGroups))
                .ToList();

            var endDayUtc = DateTime.UtcNow.Date;
            var startDayUtc = endDayUtc.AddDays(-6);
            var byDay = spRows.Where(r => r != null)
                .GroupBy(r => r.ActivityDate.Date)
                .ToDictionary(g => g.Key, g => g.First());

            var days = new List<DeviceActivityHistoryDay>(7);
            for (int i = 0; i < 7; i++)
            {
                var day = startDayUtc.AddDays(i);
                byDay.TryGetValue(day, out var row);
                days.Add(new DeviceActivityHistoryDay
                {
                    ActivityDateUtc = day,
                    TotalDevicesInScope = row?.TotalDevicesInScope ?? 0,
                    DevicesOnline = row?.DevicesOnline ?? 0,
                    DevicesOffline = row?.DevicesOffline ?? 0,
                    Label = day.ToString("dd/MM/yyyy")
                });
            }
            return new DeviceActivityHistoryModel { Days = days };
        }


        private static string NormalizeStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var v = value.Replace('–', '-').Replace('—', '-').Replace('−', '-').Trim();
            while (v.Contains("  ")) v = v.Replace("  ", " ");
            return v;
        }

        private static bool ContainsToken(string haystack, string token)
        {
            if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(token)) return false;
            return haystack.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public BatteryStatusModel GetBatteryStatus(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            _db.Database.SetCommandTimeout(500);

            var spRows = _db.Database
                .SqlQueryRaw<LatestBatteryStatusRow>("""SELECT * FROM usp_GetLatestBatteryStatus()""")
                .ToList();

            var allowedBatteryIds = GetAllowedBatteryIds(communityID, groupID, includeAllSubGroups);

            if (allowedBatteryIds.Count == 0)
                return new BatteryStatusModel { PowerModulesCount = 0, EfficiencyScorePercent = 0 };

            var rows = spRows.Where(r => allowedBatteryIds.Contains(r.Battery_ID)).ToList();
            var onDeviceCharging = 0; var onDeviceDischarging = 0; var onDeviceIdle = 0;
            var offDeviceCharging = 0; var offDeviceIdle = 0;
            var odcIds = new List<long>(); var oddIds = new List<long>(); var odiIds = new List<long>();
            var ofcIds = new List<long>(); var ofiIds = new List<long>();

            foreach (var row in rows)
            {
                var status = NormalizeStatus(row.BatteryStatus);
                if (ContainsToken(status, "On Device"))
                {
                    if (ContainsToken(status, "Discharging")) { onDeviceDischarging++; oddIds.Add(row.Battery_ID); }
                    else if (ContainsToken(status, "Charging")) { onDeviceCharging++; odcIds.Add(row.Battery_ID); }
                    else { onDeviceIdle++; odiIds.Add(row.Battery_ID); }
                }
                else if (ContainsToken(status, "Off Device"))
                {
                    if (ContainsToken(status, "Charging")) { offDeviceCharging++; ofcIds.Add(row.Battery_ID); }
                    else { offDeviceIdle++; ofiIds.Add(row.Battery_ID); }
                }
                else { offDeviceIdle++; ofiIds.Add(row.Battery_ID); }
            }

            var total = rows.Count;
            var onTotal = onDeviceCharging + onDeviceDischarging + onDeviceIdle;
            var offTotal = offDeviceCharging + offDeviceIdle;
            var efficiency = total > 0
                ? (int)Math.Round((decimal)onTotal * 100m / total, MidpointRounding.AwayFromZero) : 0;

            return new BatteryStatusModel
            {
                PowerModulesCount = allowedBatteryIds.Count,
                OnDeviceChargingCount = onDeviceCharging, OnDeviceDischargingCount = onDeviceDischarging,
                OnDeviceIdleCount = onDeviceIdle, OffDeviceChargingCount = offDeviceCharging, OffDeviceIdleCount = offDeviceIdle,
                OnDeviceTotalCount = onTotal, OffDeviceTotalCount = offTotal, EfficiencyScorePercent = efficiency,
                OnDeviceChargingBatteryIds = odcIds, OnDeviceDischargingBatteryIds = oddIds,
                OnDeviceIdleBatteryIds = odiIds, OffDeviceChargingBatteryIds = ofcIds, OffDeviceIdleBatteryIds = ofiIds
            };
        }

        #endregion

        #region AdminDashboard
        public AdminDashboardModel GetAdminDashboard()
        {
            var db = _db;
            var model = new AdminDashboardModel
            {
                Communities = db.Communities.Count(),
                Groups = db.Groups.Count(),
                RegDevices = db.Devices.Where(x => x.Community != null).Count(),
                RemDevices = db.Devices.Where(x => x.Community == null).Count(),
                RegBatteries = db.Batteries.Where(x => x.Community != null).Count(),
                RemBatteries = db.Batteries.Where(x => x.Community == null).Count()
            };
            using (var userdb = new ApplicationDbContext())
            {
                model.Users = userdb.Users.Count();
            }
            return model;
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
