using System;

namespace GeniView.Data.Hardware
{
    public class BatteryConstantData : Abstract.Data
    {
        public BatteryConstantData DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatteryConstantData n = (BatteryConstantData)this.MemberwiseClone();
            n.SerialNumber = this.SerialNumber == null ? null : string.Copy(this.SerialNumber);
            n.BatteryTechnology = this.BatteryTechnology == null ? null : string.Copy(this.BatteryTechnology);
            
            return n;
        }

        public DateTime? DateOfManufacture { get; set; }

        /// <summary>
        /// Ah
        /// </summary>
        public double DesignCapacity { get; set; }

        /// <summary>
        /// V
        /// </summary>
        public double DesignVoltage { get; set; }

        public long? SerialNumberCode { get; set; }

        public string SerialNumber { get; set; }

        public int FirmwareVersion { get; set; }

        public int BatteryConfiguration { get; set; }

        public int BatteryPackFirmwareVersion { get; set; }

        public int BatteryChemistry { get; set; }

        public string BatteryTechnology { get; set; }
    }
}
