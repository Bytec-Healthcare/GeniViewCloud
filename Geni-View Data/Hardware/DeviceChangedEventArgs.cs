using System.Collections.Generic;

namespace GeniView.Data.Hardware
{
    public class DeviceChangedEventArgs<TDevice>
    {
        public TDevice ChangedDevice { get; private set; }
        public DeviceChangedEventTypes EventType { get; private set; }
        public IEnumerable<TDevice> AllDevices { get; private set; }

        public DeviceChangedEventArgs(TDevice device, DeviceChangedEventTypes eventType, IEnumerable<TDevice> allDevices)
        {
            this.ChangedDevice = device;
            this.EventType = eventType;
            this.AllDevices = allDevices;
        }
    }
}
