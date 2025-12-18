using System.Collections.Generic;

namespace GeniView.Data.Hardware.Event
{
    public class ActiveDeviceEventsChangedEventArgs
    {
        public IEnumerable<DeviceEvent> AddedEvents { get; private set; }

        public IEnumerable<DeviceEvent> RemovedEvents { get; private set; }

        public IEnumerable<DeviceEvent> AllEvents { get; private set; }

        public ActiveDeviceEventsChangedEventArgs(IEnumerable<DeviceEvent> addedEvents, IEnumerable<DeviceEvent> removedEvents, IEnumerable<DeviceEvent> allEvents)
        {
            AddedEvents = addedEvents;
            RemovedEvents = removedEvents;
            AllEvents = allEvents;
        }
    }
}
