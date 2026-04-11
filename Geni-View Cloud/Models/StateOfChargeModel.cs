using System.Collections.Generic;

namespace GeniView.Cloud.Models
{
    public sealed class StateOfChargeModel
    {
        public int HighSoCCount { get; set; }
        public int LowSoCCount { get; set; }
        public int ChargeNowCount { get; set; }

        public int PowerModulesCount { get; set; }
        public int TotalCount { get; set; }

        public decimal HighSoCPercent { get; set; }
        public decimal LowSoCPercent { get; set; }
        public decimal ChargeNowPercent { get; set; }

        public int AverageSoC { get; set; }

        public List<long> HighSoCBatteryIds { get; set; } = new List<long>();
        public List<long> LowSoCBatteryIds { get; set; } = new List<long>();
        public List<long> ChargeNowBatteryIds { get; set; } = new List<long>();
    }
}
