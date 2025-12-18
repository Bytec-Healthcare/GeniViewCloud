using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class BatteryOperatingData : Abstract.Data
    {
        public BatteryOperatingData DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatteryOperatingData n = (BatteryOperatingData)this.MemberwiseClone();
            n.BatteryOperatingStatus = this.BatteryOperatingStatus == null ? null : this.BatteryOperatingStatus.DeepCopy();

            return n;
        }

        /// <summary>
        /// V
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// A
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// A
        /// </summary>
        public double AverageCurrent { get; set; }

        public int CycleCount { get; set; }

        public BatteryOperatingStatus BatteryOperatingStatus { get; set; }
    }
}
