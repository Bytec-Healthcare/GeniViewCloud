using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class BatteryOperatingStatus : Abstract.Data
    {
        public BatteryOperatingStatus() { }

        public BatteryOperatingStatus(int statusARaw, int statusBRaw)
        {
            StatusARaw = statusARaw & 0xF8FF; // Clear bits 8-10 that contain battery bay position.
            StatusBRaw = statusBRaw;

            // Set the corresponding values for other properties.
            StatusA = StatusARaw.HasValue && EnumHelper.IsFlagsDefined((BatteryOperatingStatesA)StatusARaw) ? (BatteryOperatingStatesA)StatusARaw : BatteryOperatingStatesA.Unknown;
            StatusAText = EnumHelper.GetFriendlyText(StatusA, ",");

            StatusB = StatusBRaw.HasValue && EnumHelper.IsFlagsDefined((BatteryOperatingStatesB)StatusBRaw) ? (BatteryOperatingStatesB)StatusBRaw : BatteryOperatingStatesB.Unknown;
            StatusBText = EnumHelper.GetFriendlyText(StatusB, ",");
        }

        public BatteryOperatingStatus DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatteryOperatingStatus n = (BatteryOperatingStatus)this.MemberwiseClone();

            n.StatusAText = this.StatusAText == null ? null : string.Copy(this.StatusAText);
            n.StatusBText = this.StatusBText == null ? null : string.Copy(this.StatusBText);

            return n;
        }

        [XmlElement("StatusAText")]
        public string StatusAText { get; set; }

        [XmlIgnore]
        public BatteryOperatingStatesA StatusA { get; set; }

        [XmlElement("StatusA")]
        public int? StatusARaw { get; set; }

        [XmlElement("StatusBText")]
        public string StatusBText { get; set; }

        [XmlIgnore]
        public BatteryOperatingStatesB StatusB { get; set; }

        [XmlElement("StatusB")]
        public int? StatusBRaw { get; set; }
    }
}
