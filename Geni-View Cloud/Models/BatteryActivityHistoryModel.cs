using System;
using System.Collections.Generic;

namespace GeniView.Cloud.Models
{
    public sealed class BatteryActivityHistoryDay
    {
        public DateTime ActivityDateUtc { get; set; }
        public int TotalBatteriesInScope { get; set; }
        public int BatteriesOnline { get; set; }
        public int BatteriesOffline { get; set; }

        // Pre-formatted label for the X axis (dd/MM/yyyy to match UX)
        public string Label { get; set; }
    }

    public sealed class BatteryActivityHistoryModel
    {
        public List<BatteryActivityHistoryDay> Days { get; set; }
    }
}
