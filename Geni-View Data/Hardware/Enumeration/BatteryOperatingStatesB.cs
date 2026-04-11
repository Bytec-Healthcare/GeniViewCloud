using System;
using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    [Flags]
    public enum BatteryOperatingStatesB
    {
        Unknown = 0x40000000,
        OK = 0,

        [Description("Over Current Detected")]
        OverCurrentDetected = 0x0001,
        [Description("End of Life Reached")]
        EndOfLifeReached = 0x0002,
        [Description("Switch Pressed")]
        SwitchPressed = 0x0004,
        [Description("Discharge Required")]
        DischargeRequired = 0x0008,
        [Description("Overcurrent Tripped")]
        OverCurrentTripped = 0x0010,
        [Description("Power Mux is Off")]
        PowerMuxOff = 0x0020,
        [Description("FET Failed")]
        FetFailed = 0x0040,
        [Description("Fuel Gauge Error")]
        FuelGaugeError = 0x0080,
        [Description("Discharging Detected")]
        DischargingDetected = 0x0100,
        [Description("State-of-Charge Threshold Final Reached")]
        StateOfChargeThresholdFinal = 0x0200,
        [Description("State-of-Charge Threshold One Reached")]
        StateOfChargeThresholdOne = 0x0400,
        [Description("Condition Flag")]
        ConditionFlag = 0x1000,
        [Description("Low Battery Voltage")]
        LowBatteryVoltage = 0x2000,
        [Description("OCV Taken")]
        OcvTaken = 0x8000
    }
}
