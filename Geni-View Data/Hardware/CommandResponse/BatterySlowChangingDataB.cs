using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class BatterySlowChangingDataB : Abstract.Data
    {
        public BatterySlowChangingDataB DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatterySlowChangingDataB n = (BatterySlowChangingDataB)this.MemberwiseClone();
            
            return n;
        }

        /// <summary>
        /// Ah
        /// </summary>
        public double PassedCharge { get; set; }

        /// <summary>
        /// Degree Celcius
        /// </summary>
        public int BatteryInternalTemperature { get; set; }

        /// <summary>
        /// Minutes
        /// </summary>
        public int AverageTimeToEmpty { get; set; }

        /// <summary>
        /// Minutes
        /// </summary>
        public int AverageTimeToFull { get; set; }

        /// <summary>
        /// 10mWh
        /// </summary>
        public int AvailableEnergy { get; set; }

        /// <summary>
        /// 10mW
        /// </summary>
        public int AvailablePower { get; set; }

        /// <summary>
        /// Degree Celcius
        /// </summary>
        public int DisplayBoardTemperature { get; set; }

        public BatteryDisplayBoardStatus DisplayBoardStatus { get; set; }

        /// <summary>
        /// Minutes
        /// </summary>
        public int LifeTime { get; set; }
    }
}
