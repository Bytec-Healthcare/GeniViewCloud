using System;
using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    [Flags]
    public enum DeviceStates
    {
        Unknown = 0x40000000,
        OK = 0,

        [Description("Standby")]
        PowerMuxOff = 0x0001,
        [Description("Battery 1 Failed Encryption")]
        BatteryOneFailedEncryption = 0x0002,        // Note: Chargers don't test encryption
        [Description("Battery 2 Failed Encryption")]
        BatteryTwoFailedEncryption = 0x0004,
        [Description("Battery 1 is Over Temperature")]
        BatteryOneOverTemperature = 0x0008,
        [Description("Battery 2 is Over Temperature")]
        BatteryTwoOverTemperature = 0x0010,
        [Description("Battery 1 is Under Temperature")]
        BatteryOneUnderTemperature = 0x0020,
        [Description("Battery 2 is Under Temperature")]
        BatteryTwoUnderTemperature = 0x0040,
        [Description("Battery 1 FET Failed")]
        BatteryOneFetFailed = 0x0080,
        [Description("Battery 2 FET Failed")]
        BatteryTwoFetFailed = 0x0100,
        [Description("Short Circuit Protection Tripped")]
        ShortCircuitProtectionTripped = 0x0200,     // Dock only
        [Description("LTC Power Mux Overcurrent Tripped")]
        LtcPowerMuxOverCurrentTripped = 0x0400,     // Dock only
        [Description("Firmware Overcurrent Warning")]
        FirmwareOverCurrentWarning = 0x0800,        // Dock only
        [Description("Firmware Overcurrent Tripped")]
        FirmwareOverCurrentTripped = 0x1000,         // Dock only
        [Description("Battery 1 OEM Identification Incorrect")]
        BatteryOneIncorrectOemIdentifier = 0x2000,
        [Description("Battery 2 OEM Identification Incorrect")]
        BatteryTwoIncorrectOemIdentifier = 0x4000
    }
}
