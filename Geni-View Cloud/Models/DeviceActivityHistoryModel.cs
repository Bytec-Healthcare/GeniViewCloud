using System;
using System.Collections.Generic;

namespace GeniView.Cloud.Models
{
    public sealed class DeviceActivityHistoryDay
    {
        public DateTime ActivityDateUtc { get; set; }
        public int TotalDevicesInScope { get; set; }
        public int DevicesOnline { get; set; }
        public int DevicesOffline { get; set; }

        // Pre-formatted label for the X axis (dd/MM/yyyy to match UX)
        public string Label { get; set; }
    }

    public sealed class DeviceActivityHistoryModel
    {
        public List<DeviceActivityHistoryDay> Days { get; set; }
    }
}
