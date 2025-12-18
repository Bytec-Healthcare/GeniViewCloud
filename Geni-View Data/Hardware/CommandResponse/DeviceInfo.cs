namespace GeniView.Data.Hardware
{
    public class DeviceInfo : Abstract.Data
    {
        public DeviceInfo DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceInfo n = (DeviceInfo)this.MemberwiseClone();
            n.FirmwareVersion = this.FirmwareVersion == null ? null : string.Copy(this.FirmwareVersion);
            n.SerialNumber = this.SerialNumber == null ? null : string.Copy(this.SerialNumber);

            return n;
        }

        public string FirmwareVersion { get; set; }

        public string SerialNumber { get; set; }

        public int OemIdentifier { get; set; }
    }
}
