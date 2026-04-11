using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    /// <summary>
    /// Converted from <see cref="BatterySystemStates"/> to reflect a single state, and not an enumeration./>
    /// </summary>
    public enum BatteryStates
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Idle")]
        Idle,
        [Description("Idle (plugged in)")]
        IdlePluggedIn,
        [Description("Charging")]
        Charging,
        [Description("Powering the system")]
        PoweringSystem
    }
}
