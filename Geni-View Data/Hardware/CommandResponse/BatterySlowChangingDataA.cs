using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class BatterySlowChangingDataA : Abstract.Data
    {
        public BatterySlowChangingDataA DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatterySlowChangingDataA n = (BatterySlowChangingDataA)this.MemberwiseClone();
            
            return n;
        }

        /// <summary>
        /// %
        /// </summary>
        public int RelativeStateOfCharge { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public int AbsoluteStateOfCharge { get; set; }

        /// <summary>
        /// Ah
        /// </summary>
        public double RemainingCapacity { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public int EndOfLifeCapacity { get; set; }

        /// <summary>
        /// Ah
        /// </summary>
        public double DischargeCapacity { get; set; }

        /// <summary>
        /// Ah
        /// </summary>
        public double CalculatedCapacity { get; set; }
    }
}
