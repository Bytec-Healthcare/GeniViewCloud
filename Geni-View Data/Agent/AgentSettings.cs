namespace GeniView.Data.Agent
{
    public class AgentSettings
    {
        public AgentSettings DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            AgentSettings n = (AgentSettings)this.MemberwiseClone();
            n.AgentID = this.AgentID == null ? null : string.Copy(this.AgentID);
            n.AgentName = this.AgentName == null ? null : string.Copy(this.AgentName);
            n.AgentDescription = this.AgentDescription == null ? null : string.Copy(this.AgentDescription);

            return n;
        }

        public string AgentID { get; set; }

        public string AgentName { get; set; }

        public string AgentDescription { get; set; }

        public bool AgentDeviceDataLoggingEnabled { get; set; }

        public uint AgentDeviceDataLoggingIntervalSeconds { get; set; }

        public bool DeviceDataPublishEnabled { get; set; }

        public uint DeviceDataPublishIntervalMinutes { get; set; }

        public bool InternalDeviceLogRetrievalEnabled { get; set; }

        public uint InternalDeviceLogRetrievalIntervalMinutes { get; set; }

        public uint DeviceCommunicationTimeoutSeconds { get; set; }

        public bool DeviceCommunicationErrorResetEnabled { get; set; }

        public bool AgentDebugLoggingEnabled { get; set; }

        public bool DeviceLocationMonitoringEnabled { get; set; }

        public bool AutomaticDeviceTimeSyncEnabled { get; set; }

        public uint AutomaticDeviceTimeSyncToleranceSeconds { get; set; }


        #region Equality Check

        public static bool operator ==(AgentSettings a, AgentSettings b)
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

        public static bool operator !=(AgentSettings a, AgentSettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            AgentSettings other = obj as AgentSettings;

            if (other == null) return false;

            return AgentID == other.AgentID &&
                   AgentName == other.AgentName &&
                   AgentDescription == other.AgentDescription &&
                   AgentDeviceDataLoggingEnabled == other.AgentDeviceDataLoggingEnabled &&
                   AgentDeviceDataLoggingIntervalSeconds == other.AgentDeviceDataLoggingIntervalSeconds &&
                   DeviceDataPublishEnabled == other.DeviceDataPublishEnabled &&
                   DeviceDataPublishIntervalMinutes == other.DeviceDataPublishIntervalMinutes &&
                   InternalDeviceLogRetrievalEnabled == other.InternalDeviceLogRetrievalEnabled &&
                   InternalDeviceLogRetrievalIntervalMinutes == other.InternalDeviceLogRetrievalIntervalMinutes &&
                   DeviceCommunicationTimeoutSeconds == other.DeviceCommunicationTimeoutSeconds &&
                   DeviceCommunicationErrorResetEnabled == other.DeviceCommunicationErrorResetEnabled &&
                   AgentDebugLoggingEnabled == other.AgentDebugLoggingEnabled &&
                   DeviceLocationMonitoringEnabled == other.DeviceLocationMonitoringEnabled &&
                   AutomaticDeviceTimeSyncEnabled == other.AutomaticDeviceTimeSyncEnabled &&
                   AutomaticDeviceTimeSyncToleranceSeconds == other.AutomaticDeviceTimeSyncToleranceSeconds;
        }

        public override int GetHashCode()
        {
            return (AgentID + AgentName + AgentDescription + AgentDeviceDataLoggingEnabled + AgentDeviceDataLoggingIntervalSeconds +
                DeviceDataPublishEnabled + DeviceDataPublishIntervalMinutes + InternalDeviceLogRetrievalEnabled + InternalDeviceLogRetrievalIntervalMinutes +
                DeviceCommunicationTimeoutSeconds + DeviceCommunicationErrorResetEnabled + AgentDebugLoggingEnabled +
                DeviceLocationMonitoringEnabled + AutomaticDeviceTimeSyncEnabled + AutomaticDeviceTimeSyncToleranceSeconds).GetHashCode();
        }

        #endregion
    }
}
