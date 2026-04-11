using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware.Event
{
    public enum DeviceEventTypes
    {
        Information,
        Warning,
        Error,
        Critical
    }

    public enum DeviceEventSources
    {
        [Description("Agent Device Log")]
        AgentDeviceLog,
        [Description("Internal Device Log")]
        InternalDeviceLog,
        [Description("G3 Log")]
        G3Log

    }

    public class DeviceEvent
    {
        public DeviceEvent() { }

        internal DeviceEvent(string description, Guid uid, DeviceEventTypes eventType, DeviceEventSources source, Func<AgentDeviceLog, InternalDeviceLog, bool> checkFunction)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException();

            if (checkFunction == null)
                throw new ArgumentNullException();

            Description = description;
            UID = uid;
            EventType = eventType;
            EventTypeText = EnumHelper.GetFriendlyText(EventType, ",");
            Source = source;
            SourceText = EnumHelper.GetFriendlyText(Source, ",");
            CheckForEventImpl = checkFunction;
        }

        public DeviceEvent DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceEvent n = (DeviceEvent)this.MemberwiseClone();
            n.DeviceSerialNumber = this.DeviceSerialNumber == null ? null : string.Copy(this.DeviceSerialNumber);
            n.Description = this.Description == null ? null : string.Copy(this.Description);
            n.EventTypeText = this.EventTypeText == null ? null : string.Copy(this.EventTypeText);
            n.SourceText = this.SourceText == null ? null : string.Copy(this.SourceText);

            return n;
        }

        public long ID { get; set; }

        public string DeviceSerialNumber { get; set; }

        [NotMapped]
        public string BatterySerialNumber { get; set; }

        public string Description { get; set; }

        public Guid UID { get; set; }

        public DeviceEventTypes EventType { get; set; }

        public string EventTypeText { get; set; }

        public DeviceEventSources Source { get; set; }

        public string SourceText { get; set; }

        public DateTime Timestamp { get; set; }

        private Func<AgentDeviceLog, InternalDeviceLog, bool> CheckForEventImpl { get; set; }

        public bool IsHandled { get; set; }

        internal bool CheckForEvent(AgentDeviceLog deviceData, InternalDeviceLog deviceLog)
        {
            return CheckForEventImpl(deviceData, deviceLog);
        }

        #region Equality Check

        public static bool operator ==(DeviceEvent a, DeviceEvent b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(DeviceEvent a, DeviceEvent b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            DeviceEvent other = obj as DeviceEvent;

            if (other == null) return false;

            return DeviceSerialNumber == other.DeviceSerialNumber &&
                   UID == other.UID;
        }

        public override int GetHashCode()
        {
            return (DeviceSerialNumber + UID).GetHashCode();
        }

        #endregion

        #region Navigation Properties
        public Nullable<long> Agent_ID { get; set; }

        public Nullable<long> Device_ID { get; set; }


        [ForeignKey("Device_ID")] //Add this attribute or we cannot update the Device_ID.
        public virtual Device Device { get; set; }

        [ForeignKey("Agent_ID")] //Add this attribute or we cannot update the Agent_ID.
        public virtual Agent.Agent Agent { get; set; }

        public void ClearNavigationProperties()
        {
            Device = null;
            Agent = null;
        }

        #endregion
    }
}
