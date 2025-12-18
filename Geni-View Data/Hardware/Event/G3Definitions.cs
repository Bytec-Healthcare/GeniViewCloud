using System;
using System.Collections.Generic;

namespace GeniView.Data.Hardware.Event
{
    public class G3Definitions
    {
        Dictionary<InternalDeviceLogEventCodes, DeviceEvent> _events = new Dictionary<InternalDeviceLogEventCodes, DeviceEvent>();

        public G3Definitions()
        {
            _events.Add(InternalDeviceLogEventCodes.BatteryAttached, new DeviceEvent(
                InternalDeviceLogEventCodes.BatteryAttached.ToString(),
                new Guid("{30DF2195-2AF9-4766-B2EB-3A7950BA3584}"),
                DeviceEventTypes.Information,
                DeviceEventSources.G3Log,
                (dd, dl) =>
                {
                    return false;
                }));

            _events.Add(InternalDeviceLogEventCodes.BatteryRemoved, new DeviceEvent(
                InternalDeviceLogEventCodes.BatteryAttached.ToString(),
                new Guid("{337893EB-B8C0-4E10-974A-16EEBDF92A2B}"),
                DeviceEventTypes.Information,
                DeviceEventSources.G3Log,
                (dd, dl) =>
                {
                    return false;
                }));


        }

        public DeviceEvent GetEvent(InternalDeviceLogEventCodes eventCode)
        {
            DeviceEvent deviceEvent = new DeviceEvent();

            switch (eventCode)
            {
                case InternalDeviceLogEventCodes.Unknown:

                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.Unknown.ToString(),
                        new Guid("{086f8108-871e-442c-9157-fa4bbe1454af}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });

                    break;
                case InternalDeviceLogEventCodes.BatteryAttached:

                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryAttached.ToString(),
                        new Guid("{30DF2195-2AF9-4766-B2EB-3A7950BA3584}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });

                    break;
                case InternalDeviceLogEventCodes.BatteryRemoved:

                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryRemoved.ToString(),
                        new Guid("{337893EB-B8C0-4E10-974A-16EEBDF92A2B}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });

                    break;
                case InternalDeviceLogEventCodes.ExternalPowerInputApplied:

                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.ExternalPowerInputApplied.ToString(),
                        new Guid("{B4E22FB3-A4C4-45BD-A80E-17A268659A85}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });

                    break;
                case InternalDeviceLogEventCodes.ExternalPowerInputRemoved:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.ExternalPowerInputRemoved.ToString(),
                        new Guid("{12D74D9D-0EA0-42EC-B19F-F15F8CA35ABF}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });

                    break;
                case InternalDeviceLogEventCodes.StandbyModeActivated:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.StandbyModeActivated.ToString(),
                        new Guid("{1A1E26D4-267A-4576-9C37-FE1D77949B8F}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.StandbyModeDeactivated:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.StandbyModeDeactivated.ToString(),
                        new Guid("{74688061-D7EA-4BA6-A3B8-A24A614863AB}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.LowBatteryWarning:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.LowBatteryWarning.ToString(),
                        new Guid("{FC9C06CD-5B49-4761-90A2-4A86150FDBA0}"),
                        DeviceEventTypes.Warning,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.BatteryAuthenticationFailure:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryAuthenticationFailure.ToString(),
                        new Guid("{E838C770-AEA9-400B-BB37-321CAD14FFCE}"),
                        DeviceEventTypes.Error,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.LoadPresent:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.LoadPresent.ToString(),
                        new Guid("{7F9E2EBA-05AE-4A34-B6B1-27977E2C5C30}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.LoadRemoved:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.LoadRemoved.ToString(),
                        new Guid("{B38D5D53-70E1-453F-9BA0-D1099CE3218A}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.OverloadWarning:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.OverloadWarning.ToString(),
                        new Guid("{F8D0E91C-2150-48E0-82A4-CD21EF272130}"),
                        DeviceEventTypes.Error,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.LowTemperatureWarning:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.LowTemperatureWarning.ToString(),
                        new Guid("{81B6E8F6-D2A7-4E56-92EB-F82400B23AA9}"),
                        DeviceEventTypes.Error,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.HighTemperatureWarning:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.HighTemperatureWarning.ToString(),
                        new Guid("{64C99D9F-F37E-458A-B1E3-8C73D574A903}"),
                        DeviceEventTypes.Error,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.SMBusCommunicationIssue:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.SMBusCommunicationIssue.ToString(),
                        new Guid("{C37937F0-9378-42D8-B3A9-D8C2758215AC}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.SystemPowerUp:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.SystemPowerUp.ToString(),
                        new Guid("{039A1FDE-8EFF-4BB6-B87B-F6340FB57A2E}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.SystemPowerDown:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.SystemPowerDown.ToString(),
                        new Guid("{7E862E40-BD1C-4A4F-8DC0-37E0F507EE0D}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.AutoPowerDown:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.AutoPowerDown.ToString(),
                        new Guid("{1A4296CC-B977-43BD-910F-C53914FC68FF}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.LostCommsWithFuelGauge:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.LostCommsWithFuelGauge.ToString(),
                        new Guid("{1752A0DE-1DD8-46BF-8F51-156D2B361B7B}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.FactoryDefaultsReset:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.FactoryDefaultsReset.ToString(),
                        new Guid("{17744954-240F-4E4A-A2B9-011F004F078E}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.SerialNumberUpdated:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.SerialNumberUpdated.ToString(),
                        new Guid("{E000A3A2-D619-4475-B56A-FE5612D91860}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.SetDevicetoFirmwareMode:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.SetDevicetoFirmwareMode.ToString(),
                        new Guid("{E36D8342-B648-47C4-BC9C-33A2D603708B}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.SyncDeviceClock:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.SyncDeviceClock.ToString(),
                        new Guid("{69127B83-A6A2-49B2-A0A3-513B7B96D0C8}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.TimeTrigger:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.TimeTrigger.ToString(),
                        new Guid("{12a2c7d6-4bad-4dec-a5b8-591211f7dee8}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.BatteryOneSwitchOff:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryOneSwitchOff.ToString(),
                        new Guid("{541F52AB-C575-4576-AE7B-D647F2C81C11}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.BatteryTwoSwitchOff:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryTwoSwitchOff.ToString(),
                        new Guid("{3B0A4528-E073-4677-9170-8F31EE8529D7}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.FETSwitchOffOne:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.FETSwitchOffOne.ToString(),
                        new Guid("{4E13901C-45B7-4157-ABBA-B3D7A82F878B}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.FETSwitchOffTwo:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.FETSwitchOffTwo.ToString(),
                        new Guid("{501D0BB1-F15F-4680-B5A5-48CBC1FEB65B}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.ServiceIndicatorReset:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.ServiceIndicatorReset.ToString(),
                        new Guid("{EEBBBA37-67FA-49E8-B3D0-8A1B7E78009A}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.OemIdCheckFailed:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.OemIdCheckFailed.ToString(),
                        new Guid("{1A6833A3-ADD1-47FE-A7A3-8917F61FBC78}"),
                        DeviceEventTypes.Critical,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.BatteryOneServiceIndicatorReset:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryOneServiceIndicatorReset.ToString(),
                        new Guid("{1E8DACFA-E5CA-4C02-9A28-610AF6E8C541}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.BatteryTwoServiceIndicatorReset:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryTwoServiceIndicatorReset.ToString(),
                        new Guid("{A806710D-B1E9-4D50-BBB0-8E85938BCC20}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.BatteryThreeServiceIndicatorReset:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryThreeServiceIndicatorReset.ToString(),
                        new Guid("{337893EB-B8C0-4E10-974A-16EEBDF92A2B}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.BatteryFourServiceIndicatorReset:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryFourServiceIndicatorReset.ToString(),
                        new Guid("{BE006E23-E7FF-4D11-8724-B76A29D88226}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.BatteryFiveServiceIndicatorReset:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.BatteryFiveServiceIndicatorReset.ToString(),
                        new Guid("{40FBCAD4-4955-49C6-AC5B-CDC8EC5EB2BB}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.G3BatteryAdded:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.G3BatteryAdded.ToString(),
                        new Guid("{18998d1a-1e82-43da-ad00-d23e73892d77}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.G3BatteryRemoved:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.G3BatteryRemoved.ToString(),
                        new Guid("{2049fbd4-6543-4929-af04-5d3253f577fe}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.G3DockUnscheduledShutdown:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.G3DockUnscheduledShutdown.ToString(),
                        new Guid("{2fcd8bb5-aaca-4cba-9516-4c43c219b334}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.G3BatteryUnscheduledShutdown:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.G3BatteryUnscheduledShutdown.ToString(),
                        new Guid("{0a0c0d3e-3bae-4967-8bee-3e08ee0eba74}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                case InternalDeviceLogEventCodes.DataUpload:
                    deviceEvent = new DeviceEvent(
                        InternalDeviceLogEventCodes.DataUpload.ToString(),
                        new Guid("{337893EB-B8C0-4E10-974A-16EEBDF92A2B}"),
                        DeviceEventTypes.Information,
                        DeviceEventSources.G3Log,
                        (dd, dl) =>
                        {
                            return false;
                        });
                    break;
                default:
                    break;
            }

            return deviceEvent;
        }


    }
}
