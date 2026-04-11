using System;

namespace GeniView.Cloud.Models
{
    public sealed class DashboardPopupRowModel
    {
        public long Battery_ID { get; set; }
        public string PowerModules { get; set; }
        public string AttachedTo { get; set; }
        public string DeviceType { get; set; }
        public int? SoC { get; set; }
        public int? CycleCount { get; set; }
        public int? Temperature { get; set; }
        public string Status { get; set; }

        // SP returns '-' or a DeviceSerialNumber string, not a datetime.
        public string LastAttached { get; set; }

        public DateTime? LastCharged { get; set; }
        public DateTime? LastDischarged { get; set; }
    }
}