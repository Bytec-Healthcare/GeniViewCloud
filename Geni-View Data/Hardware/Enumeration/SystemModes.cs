using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    public enum SystemModes
    {
        Disabled = 0,
        [Description("LED Alert")]
        LEDAlert = 1,
        [Description("Buzzer Alert")]
        BuzzerAlert = 2,
        All = 3
    }
}
