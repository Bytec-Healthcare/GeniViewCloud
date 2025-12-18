using System;
using System.Collections.Generic;

namespace GeniView.Data.Hardware.Event
{
    internal class DeviceEventDefinitions
    {
        public IList<DeviceEvent> DeviceEvents { get; private set; }

        public DeviceEventDefinitions()
        {
            DeviceEvents = new List<DeviceEvent>();

            #region From Internal Device Logs

            DeviceEvents.Add(new DeviceEvent("Battery Attached",
                new Guid("{30DF2195-2AF9-4766-B2EB-3A7950BA3584}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryAttached)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery Removed",
                new Guid("{337893EB-B8C0-4E10-974A-16EEBDF92A2B}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryRemoved)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("External Power Input Applied",
                new Guid("{B4E22FB3-A4C4-45BD-A80E-17A268659A85}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.ExternalPowerInputApplied)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("External Power Input Removed",
                new Guid("{12D74D9D-0EA0-42EC-B19F-F15F8CA35ABF}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.ExternalPowerInputRemoved)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Standby Mode Activated",
                new Guid("{1A1E26D4-267A-4576-9C37-FE1D77949B8F}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.StandbyModeActivated)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Standby Mode Deactivated",
                new Guid("{74688061-D7EA-4BA6-A3B8-A24A614863AB}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.StandbyModeDeactivated)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Low Battery Warning",
                new Guid("{FC9C06CD-5B49-4761-90A2-4A86150FDBA0}"),
                DeviceEventTypes.Warning,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.LowBatteryWarning)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery Authentication Failure",
                new Guid("{E838C770-AEA9-400B-BB37-321CAD14FFCE}"),
                DeviceEventTypes.Error,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryAuthenticationFailure)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Load Present",
                new Guid("{7F9E2EBA-05AE-4A34-B6B1-27977E2C5C30}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.LoadPresent)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Load Removed",
                new Guid("{B38D5D53-70E1-453F-9BA0-D1099CE3218A}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.LoadRemoved)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Overload Warning",
                new Guid("{F8D0E91C-2150-48E0-82A4-CD21EF272130}"),
                DeviceEventTypes.Error,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.OverloadWarning)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Low Temperature Warning",
                new Guid("{81B6E8F6-D2A7-4E56-92EB-F82400B23AA9}"),
                DeviceEventTypes.Error,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.LowTemperatureWarning)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("High Temperature Warning",
                new Guid("{64C99D9F-F37E-458A-B1E3-8C73D574A903}"),
                DeviceEventTypes.Error,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.HighTemperatureWarning)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("SM Bus Communication Issue",
                new Guid("{C37937F0-9378-42D8-B3A9-D8C2758215AC}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.SMBusCommunicationIssue)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("System Power-up",
                new Guid("{039A1FDE-8EFF-4BB6-B87B-F6340FB57A2E}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.SystemPowerUp)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("System Power-down",
                new Guid("{7E862E40-BD1C-4A4F-8DC0-37E0F507EE0D}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.SystemPowerDown)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Automatic Power-down",
                new Guid("{1A4296CC-B977-43BD-910F-C53914FC68FF}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.AutoPowerDown)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Lost Comms with Fuel Gauge",
                new Guid("{1752A0DE-1DD8-46BF-8F51-156D2B361B7B}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.LostCommsWithFuelGauge)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Factory Defaults Reset",
                new Guid("{17744954-240F-4E4A-A2B9-011F004F078E}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.FactoryDefaultsReset)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Serial Number Updated",
                new Guid("{E000A3A2-D619-4475-B56A-FE5612D91860}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.SerialNumberUpdated)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Set Device to Firmware Mode",
                new Guid("{E36D8342-B648-47C4-BC9C-33A2D603708B}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.SetDevicetoFirmwareMode)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Sync Device Clock",
                new Guid("{69127B83-A6A2-49B2-A0A3-513B7B96D0C8}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.SyncDeviceClock)
                        return true;
                    else
                        return false;
                }));

            //DeviceEvents.Add(new DeviceEvent("Periodic Data Logging Triggered",
            //    new Guid("{8AF4DDB7-0510-478A-ADF6-DCC72ED992A9}"),
            //    DeviceEventTypes.Information,
            //    DeviceEventSource.InternalDeviceLog,
            //    (dd, dl) =>
            //    {
            //        if (dl != null && dl.EventCode == DeviceLogEventCodes.TimeTrigger)
            //            return true;
            //        else
            //            return false;
            //    }));

            DeviceEvents.Add(new DeviceEvent("Battery 1 Switch OFF",
                new Guid("{541F52AB-C575-4576-AE7B-D647F2C81C11}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryOneSwitchOff)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 2 Switch OFF",
                new Guid("{3B0A4528-E073-4677-9170-8F31EE8529D7}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryTwoSwitchOff)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("FET 1 Switch OFF",
                new Guid("{4E13901C-45B7-4157-ABBA-B3D7A82F878B}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.FETSwitchOffOne)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("FET 2 Switch OFF",
                new Guid("{501D0BB1-F15F-4680-B5A5-48CBC1FEB65B}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.FETSwitchOffTwo)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Service Indicator Reset",
                new Guid("{EEBBBA37-67FA-49E8-B3D0-8A1B7E78009A}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.ServiceIndicatorReset)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("OEM ID Check Failed",
                new Guid("{1A6833A3-ADD1-47FE-A7A3-8917F61FBC78}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.OemIdCheckFailed)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 1 Service Indicator Reset",
                new Guid("{1E8DACFA-E5CA-4C02-9A28-610AF6E8C541}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryOneServiceIndicatorReset)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 2 Service Indicator Reset",
                new Guid("{A806710D-B1E9-4D50-BBB0-8E85938BCC20}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryTwoServiceIndicatorReset)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 3 Service Indicator Reset",
                new Guid("{D64D1511-5E93-4C51-BAF4-E4C676239AB2}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryThreeServiceIndicatorReset)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 4 Service Indicator Reset",
                new Guid("{BE006E23-E7FF-4D11-8724-B76A29D88226}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryFourServiceIndicatorReset)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 5 Service Indicator Reset",
                new Guid("{40FBCAD4-4955-49C6-AC5B-CDC8EC5EB2BB}"),
                DeviceEventTypes.Information,
                DeviceEventSources.InternalDeviceLog,
                (dd, dl) =>
                {
                    if (dl != null && dl.EventCode == InternalDeviceLogEventCodes.BatteryFiveServiceIndicatorReset)
                        return true;
                    else
                        return false;
                }));

            //DeviceEvents.Add(new DeviceEvent("Data Upload",
            //    new Guid("{5DCF3CD5-81A6-47C6-AFD3-300C0B02071D}"),
            //    DeviceEventTypes.Information,
            //    DeviceEventSource.InternalDeviceLog,
            //    (dd, dl) =>
            //    {
            //        if (dl != null && dl.EventCode == DeviceLogEventCodes.DataUpload)
            //            return true;
            //        else
            //            return false;
            //    }));

            #endregion

            #region From Agent Device Logs

            DeviceEvents.Add(new DeviceEvent("Battery 1 Failed Encryption",
                new Guid("{5EE641CF-A90E-4A4D-A674-2004B2330C5E}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryOneFailedEncryption) == DeviceStates.BatteryOneFailedEncryption)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 2 Failed Encryption",
                new Guid("{BA51396C-713F-4C72-9E34-821902C2954A}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryTwoFailedEncryption) == DeviceStates.BatteryTwoFailedEncryption)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 1 Over Temperature",
                new Guid("{3D902146-4CF4-48C5-A866-CD60EF724FE0}"),
                DeviceEventTypes.Error,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryOneOverTemperature) == DeviceStates.BatteryOneOverTemperature)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 2 Over Temperature",
                new Guid("{8748D6FC-5398-426A-8833-8207DAFFEE43}"),
                DeviceEventTypes.Error,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryTwoOverTemperature) == DeviceStates.BatteryTwoOverTemperature)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 1 Under Temperature",
                new Guid("{D335ABCF-2D9F-4DC9-A74A-40CFA6BE6BBB}"),
                DeviceEventTypes.Error,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryOneUnderTemperature) == DeviceStates.BatteryOneUnderTemperature)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 2 Under Temperature",
                new Guid("{61589C6A-BF7C-4451-A1EA-5095EA336699}"),
                DeviceEventTypes.Error,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryTwoUnderTemperature) == DeviceStates.BatteryTwoUnderTemperature)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 1 FET Failed",
                new Guid("{F5DF6384-6122-45E0-B52C-D5C09831269D}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryOneFetFailed) == DeviceStates.BatteryOneFetFailed)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 2 FET Failed",
                new Guid("{A4CDD5C9-E185-4571-BFD9-5F46AE238BAA}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryTwoFetFailed) == DeviceStates.BatteryTwoFetFailed)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Short Circuit Protection Tripped",
                new Guid("{FB4C31C9-C80C-4E91-B69C-A24676A77EFC}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.ShortCircuitProtectionTripped) == DeviceStates.ShortCircuitProtectionTripped)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("LTC Overcurrent Protection Tripped",
                new Guid("{96488729-20BB-4B56-A6A7-6D9E563BA9B1}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.LtcPowerMuxOverCurrentTripped) == DeviceStates.LtcPowerMuxOverCurrentTripped)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Firmware Overcurrent Detected",
                new Guid("{234F2890-3AA6-4FCF-97F5-C8CEAD62732E}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.FirmwareOverCurrentWarning) == DeviceStates.FirmwareOverCurrentWarning)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Firmware Overcurrent Tripped",
                new Guid("{4562ADE9-740C-4AB5-A3E5-0ECD39D76E43}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.FirmwareOverCurrentTripped) == DeviceStates.FirmwareOverCurrentTripped)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 1 OEM Identification Incorrect",
                new Guid("{56D10889-B928-41FC-8C4F-CD1A48940F83}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryOneIncorrectOemIdentifier) == DeviceStates.BatteryOneIncorrectOemIdentifier)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Battery 2 OEM Identification Incorrect",
                new Guid("{427E8889-240A-41B2-8643-AF49A6DD46F9}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if ((dd.Status.Status & DeviceStates.BatteryTwoIncorrectOemIdentifier) == DeviceStates.BatteryTwoIncorrectOemIdentifier)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Device Power Level Low",
                new Guid("{44DD2CFF-CCF0-4DC5-A36B-3F2CD138A75B}"),
                DeviceEventTypes.Warning,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    // No external power and capacity is less than or equal to low level threshold but higher than critical.
                    if (dd.IsExternalPowerInputApplied == false && dd.DeviceCapacity <= 20 && dd.DeviceCapacity > 5)
                        return true;
                    else
                        return false;
                }));

            DeviceEvents.Add(new DeviceEvent("Device Power Level Critical",
                new Guid("{1E8E3422-24F3-4064-A9EC-DE3C0EF6C063}"),
                DeviceEventTypes.Critical,
                DeviceEventSources.AgentDeviceLog,
                (dd, dl) =>
                {
                    if (dd.IsExternalPowerInputApplied == false && dd.DeviceCapacity <= 5)
                        return true;
                    else
                        return false;
                }));

            #endregion
        }
    }
}
