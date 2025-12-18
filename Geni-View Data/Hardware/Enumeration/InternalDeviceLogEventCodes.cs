using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    public enum InternalDeviceLogEventCodes
    {
        Unknown = 0,

        [Description("Battery Attached")]
        BatteryAttached = 1,
        [Description("Battery Removed")]
        BatteryRemoved = 2,
        [Description("External Power Input Applied")]
        ExternalPowerInputApplied = 3,
        [Description("External Power Input Removed")]
        ExternalPowerInputRemoved = 4,
        [Description("Standby Mode Activated")]
        StandbyModeActivated = 5,
        [Description("Standby Mode Deactivated")]
        StandbyModeDeactivated = 6,
        [Description("Low Battery Warning")]
        LowBatteryWarning = 7,
        [Description("Battery Authentication Failure")]
        BatteryAuthenticationFailure = 8,
        [Description("Load Present")]
        LoadPresent = 9,
        [Description("Load Removed")]
        LoadRemoved = 10,
        [Description("Overload Warning")]
        OverloadWarning = 11,
        [Description("Low Temperature Warning")]
        LowTemperatureWarning = 15,
        [Description("High Temperature Warning")]
        HighTemperatureWarning = 16,
        [Description("SM Bus Communication Issue")]
        SMBusCommunicationIssue = 17,
        [Description("System Power-up")]
        SystemPowerUp = 18,
        [Description("System Power-down")]
        SystemPowerDown = 19,
        [Description("Automatic Power-down")]
        AutoPowerDown = 20,
        [Description("Lost Comms with Fuel Gauge")]
        LostCommsWithFuelGauge = 21,
        [Description("Factory Defaults Reset")]
        FactoryDefaultsReset = 31,
        [Description("Serial Number Updated")]
        SerialNumberUpdated = 32,
        [Description("Set Device to Firmware Mode")]
        SetDevicetoFirmwareMode = 33,
        [Description("Sync Device Clock")]
        SyncDeviceClock = 34,
        [Description("Periodic Data Logging Triggered")]
        TimeTrigger = 50,
        [Description("Battery 1 Switch OFF")]
        BatteryOneSwitchOff = 60,
        [Description("Battery 2 Switch OFF")]
        BatteryTwoSwitchOff = 61,
        [Description("FET 1 Switch OFF")]
        FETSwitchOffOne = 62,
        [Description("FET 2 Switch OFF")]
        FETSwitchOffTwo = 63,
        [Description("Service Indicator Reset")]
        ServiceIndicatorReset = 64,
        [Description("OEM ID Check Failed")]
        OemIdCheckFailed = 65,
        [Description("Battery 1 Service Indicator Reset")]
        BatteryOneServiceIndicatorReset = 66,
        [Description("Battery 2 Service Indicator Reset")]
        BatteryTwoServiceIndicatorReset = 67,
        [Description("Battery 3 Service Indicator Reset")]
        BatteryThreeServiceIndicatorReset = 68,
        [Description("Battery 4 Service Indicator Reset")]
        BatteryFourServiceIndicatorReset = 69,
        [Description("Battery 5 Service Indicator Reset")]
        BatteryFiveServiceIndicatorReset = 70,
        [Description("Battery Added")]
        G3BatteryAdded = 71,
        [Description("Battery Removed")]
        G3BatteryRemoved = 72,
        [Description("Unscheduled Shutdown (Dock)")]
        G3DockUnscheduledShutdown = 75,
        [Description("Unscheduled Shutdown (PM)")]
        G3BatteryUnscheduledShutdown = 76,
        [Description("Data Upload")]
        DataUpload = 99
    }
}
