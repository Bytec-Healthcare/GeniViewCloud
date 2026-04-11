using System.Collections.Generic;

namespace GeniView.Data.Hardware.Event
{
    public class DeviceEventChecker
    {
        private DeviceEventDefinitions _eventDefinitions = new DeviceEventDefinitions();

        public IEnumerable<DeviceEvent> CheckForEvents(string deviceSerialNumber, AgentDeviceLog deviceData, InternalDeviceLog deviceLog)
        {
            var deviceEvents = new List<DeviceEvent>();

            foreach (var item in _eventDefinitions.DeviceEvents)
            {
                if (item.CheckForEvent(deviceData, deviceLog))
                {
                    // If not deep copied, properties will be global causing undesirable effects (IsHandled true etc.)
                    var ev = item.DeepCopy();
                    ev.DeviceSerialNumber = deviceSerialNumber;

                    // Set event time based on source.
                    switch (ev.Source)
                    {
                        case DeviceEventSources.AgentDeviceLog:
                            ev.Timestamp = deviceData.Timestamp;
                            break;
                        case DeviceEventSources.InternalDeviceLog:
                            ev.Timestamp = deviceLog.Timestamp;
                            break;
                        default:
                            break;
                    }

                    deviceEvents.Add(ev);
                }
            }

            return deviceEvents;
        }
    }
}
