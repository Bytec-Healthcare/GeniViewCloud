using System;

namespace GeniView.Data.Hardware
{
    [Serializable]
    public class DeviceException : Exception
    {
        public DeviceException() { }
        public DeviceException(string message) : base(message) { }
        public DeviceException(string message, Exception inner) : base(message, inner) { }
        protected DeviceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
