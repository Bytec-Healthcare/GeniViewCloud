using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    public class DeviceBay : Abstract.Data
    {
        public DeviceBay DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceBay n = (DeviceBay)this.MemberwiseClone();
            n.Battery = this.Battery == null ? null : this.Battery.DeepCopy();

            return n;
        }

        [NotMapped]
        public int Bay { get; set; }

        public Battery Battery { get; set; }

        public bool IsBatteryPresent { get; set; }
    }
}
