using System;

namespace GeniView.Cloud.Models
{
    public sealed class BatteryStatusModel
    {
        public int PowerModulesCount { get; set; }

        public int OnDeviceChargingCount { get; set; }
        public int OnDeviceDischargingCount { get; set; }
        public int OnDeviceIdleCount { get; set; }

        public int OffDeviceChargingCount { get; set; }
        public int OffDeviceIdleCount { get; set; }

        public int OnDeviceTotalCount { get; set; }
        public int OffDeviceTotalCount { get; set; }

        public int EfficiencyScorePercent { get; set; }

        // For the segmented UX bar
        public decimal OnDeviceChargingPercent { get; set; }
        public decimal OnDeviceDischargingPercent { get; set; }
        public decimal OnDeviceIdlePercent { get; set; }
        public decimal OffDeviceChargingPercent { get; set; }
        public decimal OffDeviceIdlePercent { get; set; }
    }
}
