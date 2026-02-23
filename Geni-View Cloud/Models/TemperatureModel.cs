namespace GeniView.Cloud.Models
{
    public sealed class TemperatureModel
    {
        public int PowerModulesCount { get; set; }

        public int EfficiencyScorePercent { get; set; }

        public int ChargingNormalCount { get; set; }
        public int ChargingWarningCount { get; set; }

        public int DischargingNormalCount { get; set; }
        public int DischargingWarningCount { get; set; }

        public int TotalValidTempCount { get; set; }
        public decimal NormalPercent { get; set; }
        public decimal WarningPercent { get; set; }

        // Battery_ID buckets for popup
        public System.Collections.Generic.List<long> ChargingNormalBatteryIds { get; set; } = new System.Collections.Generic.List<long>();
        public System.Collections.Generic.List<long> ChargingWarningBatteryIds { get; set; } = new System.Collections.Generic.List<long>();
        public System.Collections.Generic.List<long> DischargingNormalBatteryIds { get; set; } = new System.Collections.Generic.List<long>();
        public System.Collections.Generic.List<long> DischargingWarningBatteryIds { get; set; } = new System.Collections.Generic.List<long>();
    }
}
