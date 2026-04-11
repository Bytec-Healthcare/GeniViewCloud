using GeniView.Data;
using GeniView.Data.Hardware;
using System;
using System.Collections.Generic;

namespace GeniView.Data.Hardware
{
    public static class DeviceDataHelpers
    {
        //public static DateTime? BatteryDateToDateTime(ushort date)
        //{
        //    // The date from the battery is in a special packed format. 
        //    // (year - 1980) * 512 + month * 32 + day. 
        //    int day = date & 0x1F;                      // bits 0-4, 5 bit value
        //    int month = (date >> 5) & 0x0F;             // bits 5-8, 4 bit value
        //    int year = ((date >> 9) & 0x7F) + 1980;     // bits 9-15, 7 bit value

        //    try
        //    {
        //        return new DateTime(year, month, day);
        //    }
        //    catch (Exception)
        //    {
        //        // Creating DateTime failed. Happens when battery is new with no set date.
        //        return null;
        //    }
        //}

        //public static BatterySystemStates ParseLtcData(string batterySystemState, string batterySystemStateCont)
        //{
        //    ushort batteryState = ushort.Parse(batterySystemState, System.Globalization.NumberStyles.HexNumber);          // 16-bit value
        //    ushort batteryStateCont = ushort.Parse(batterySystemStateCont, System.Globalization.NumberStyles.HexNumber);  // 16 bit-value

        //    batteryState &= 0x0333;     // Clear unused bits.
        //    batteryStateCont &= 0x0003; // Clear unused bits.

        //    // Add batterystatecont to higher 16 bits.
        //    int state = batteryStateCont;
        //    state = state << 16;

        //    // Add batterystate.
        //    state = state | batteryState;

        //    return (BatterySystemStates)state;
        //}

        //public static BatteryDisplayBoardStatus ParseDisplayBoardStatus(ushort status)
        //{
        //    BatteryDisplayBoardStatus s = new BatteryDisplayBoardStatus();
        //    s.StatusRaw = status;

        //    s.PerceivedStateOfCharge = status & 0x7F;                             // bits 0-6
        //    s.ActiveLEDs = (status & 0xF00) >> 8;                                 // bits 8-11
        //    s.IsDisplayFlashing = (status & 0x1000) == 0x1000;                    // bit 12
        //    s.IsDisplayRed = (status & 0x2000) == 0x2000;                         // bit 13

        //    var chargeMode = ((status & 0xC000) >> 14);                           // bits 14-15

        //    s.ChargeMode = Enum.IsDefined(typeof(FuelGaugeChargeModes), chargeMode) ? (FuelGaugeChargeModes)chargeMode : FuelGaugeChargeModes.Unknown;

        //    return s;
        //}

        //public static DeviceAlertSettings ParseDeviceAlertSettings(ushort alarmSettings)
        //{
        //    int alarms = alarmSettings & 0x03;                            // bits 0-1
        //    int displayMode = (alarmSettings >> 2) & 0x01;                // bit 2
        //    int systemModes = (alarmSettings >> 4) & 0x03;                // bits 4-5
        //    int lowBatAlert = (alarmSettings >> 8) & 0x0F;                // bits 8-11
        //    int lowBatAlertInterval = (alarmSettings >> 12) & 0x0F;       // bits 12-15

        //    // Make sure enums are valid.
        //    if (Enum.IsDefined(typeof(DeviceAlertTypes), alarms) == false)
        //        throw new Exception("Unexpected value is returned.");

        //    if (Enum.IsDefined(typeof(DisplayModes), displayMode) == false)
        //        throw new Exception("Unexpected value is returned.");

        //    if (Enum.IsDefined(typeof(SystemModes), systemModes) == false)
        //        throw new Exception("Unexpected value is returned.");

        //    return new DeviceAlertSettings()
        //    {
        //        AlertType = (DeviceAlertTypes)alarms,
        //        DisplayMode = (DisplayModes)displayMode,
        //        SystemMode = (SystemModes)systemModes,
        //        LowBatteryAlertLevel = lowBatAlert * 5, // 5 minute steps.
        //        LowBatteryAlertInterval = lowBatAlertInterval * 10 // 10 second steps.
        //    };
        //}

        //public static ushort CreateDeviceAlertSettingsValue(DeviceAlertSettings alarmSettings)
        //{
        //    ushort newSettings = 0;

        //    newSettings = (ushort)((alarmSettings.LowBatteryAlertInterval / 10) << 12);         // bits 12-15, 10 second steps.
        //    newSettings |= (ushort)((alarmSettings.LowBatteryAlertLevel / 5) << 8);             // bits 8-11,  5 minute steps.
        //    newSettings |= (ushort)((ushort)alarmSettings.SystemMode << 4);            // bits 4-5
        //    newSettings |= (ushort)((ushort)alarmSettings.DisplayMode << 2);                    // bit 2
        //    newSettings |= (ushort)alarmSettings.AlertType;                                    // bits 0-1

        //    return newSettings;
        //}

        //public static InternalDeviceLog ParseDeviceLog(string[] dataSegments, int readIndexOffset, string deviceSerialNumber)
        //{
        //    const int DEVICELOGSIZE = 11;
        //    const int BATTERYLOGSIZE = 14;

        //    // Two ways to determine the battery log count within the data: 1) count the data segments, 2) use log size returned from device itself.
        //    int numberOfBatteryLogs = (dataSegments.Length - 1 - readIndexOffset - DEVICELOGSIZE) / BATTERYLOGSIZE;
        //    // int logSize = short.Parse(dataSegments[readIndexOffset + 1], System.Globalization.NumberStyles.HexNumber);

        //    InternalDeviceLog dl = new InternalDeviceLog(ushort.Parse(dataSegments[readIndexOffset + 3], System.Globalization.NumberStyles.HexNumber),
        //        ushort.Parse(dataSegments[readIndexOffset + 5], System.Globalization.NumberStyles.HexNumber),
        //        ushort.Parse(dataSegments[readIndexOffset + 6], System.Globalization.NumberStyles.HexNumber),
        //        ushort.Parse(dataSegments[readIndexOffset + 7], System.Globalization.NumberStyles.HexNumber),
        //        ushort.Parse(dataSegments[readIndexOffset + 8], System.Globalization.NumberStyles.HexNumber),
        //        short.Parse(dataSegments[readIndexOffset + 11], System.Globalization.NumberStyles.HexNumber))
        //    {
        //        LogIndex = uint.Parse(dataSegments[readIndexOffset + 2], System.Globalization.NumberStyles.HexNumber),
        //        Timestamp = DeviceDataHelpers.UnixTimeToDateTime(uint.Parse(dataSegments[readIndexOffset + 4], System.Globalization.NumberStyles.HexNumber)),
        //        PowerOutput = new DevicePower()
        //        {
        //            Current = ushort.Parse(dataSegments[readIndexOffset + 9], System.Globalization.NumberStyles.HexNumber) / 1000.00, // Convert mA -> A
        //            Voltage = ushort.Parse(dataSegments[readIndexOffset + 10], System.Globalization.NumberStyles.HexNumber) / 1000.00, // Convert mV -> V
        //        },

        //        DeviceSerialNumber = deviceSerialNumber
        //    };

        //    if (numberOfBatteryLogs > 0)
        //    {
        //        dl.InternalBatteryLogs = new List<InternalBatteryLog>();
        //        readIndexOffset = readIndexOffset + DEVICELOGSIZE;

        //        for (int i = 0; i < numberOfBatteryLogs; i++)
        //        {
        //            readIndexOffset += (i * BATTERYLOGSIZE);

        //            InternalBatteryLog bl = new InternalBatteryLog(dl.EventCodeRaw,
        //                ushort.Parse(dataSegments[readIndexOffset + 11], System.Globalization.NumberStyles.HexNumber),
        //                ushort.Parse(dataSegments[readIndexOffset + 12], System.Globalization.NumberStyles.HexNumber))
        //            {
        //                LogIndex = uint.Parse(dataSegments[readIndexOffset + 1], System.Globalization.NumberStyles.HexNumber),
        //                Timestamp = dl.Timestamp,
        //                BatterySerialNumberCode = uint.Parse(dataSegments[readIndexOffset + 2], System.Globalization.NumberStyles.HexNumber),
        //                BatterySerialNumber = DataHelpers.BatterySerialNumberCodeToSerialNumber(uint.Parse(dataSegments[readIndexOffset + 2], System.Globalization.NumberStyles.HexNumber)),
        //                BatteryFirmwareVersion = ushort.Parse(dataSegments[readIndexOffset + 3], System.Globalization.NumberStyles.HexNumber),
        //                DeviceSerialNumber = deviceSerialNumber,
        //                DeviceLogIndex = dl.LogIndex,
        //                Voltage = ushort.Parse(dataSegments[readIndexOffset + 4], System.Globalization.NumberStyles.HexNumber) / 1000.00, // Convert to mV -> V
        //                Current = short.Parse(dataSegments[readIndexOffset + 5], System.Globalization.NumberStyles.HexNumber) / 1000.00, // Convert mA -> A
        //                AverageCurrent = short.Parse(dataSegments[readIndexOffset + 6], System.Globalization.NumberStyles.HexNumber) / 1000.00, // Convert mA -> A
        //                RelativeStateOfCharge = ushort.Parse(dataSegments[readIndexOffset + 7], System.Globalization.NumberStyles.HexNumber),
        //                RemainingCapacity = ushort.Parse(dataSegments[readIndexOffset + 8], System.Globalization.NumberStyles.HexNumber) / 1000.00, // Convert mAh -> Ah
        //                OemIdentifier = ushort.Parse(dataSegments[readIndexOffset + 9], System.Globalization.NumberStyles.HexNumber),
        //                CycleCount = ushort.Parse(dataSegments[readIndexOffset + 10], System.Globalization.NumberStyles.HexNumber),
        //                Temperature = short.Parse(dataSegments[readIndexOffset + 13], System.Globalization.NumberStyles.HexNumber),
        //                CalculatedCapacity = ushort.Parse(dataSegments[readIndexOffset + 14], System.Globalization.NumberStyles.HexNumber) / 1000.00, // Convert mAh -> Ah
        //                Source = InternalBatteryLogSources.Device // This log is from Device
        //            };

        //            // Set unique index.
        //            bl.UniqueLogIndex = bl.LogIndex.ToString() + bl.DeviceLogIndex.ToString() + "D"; // "D" for Device sourced.

        //            // Determine battery state.
        //            bl.Status = SetInternalBatteryLogStatus(bl.Bay, dl.BatteriesPoweringSystem, dl.BatteriesCharging);
        //            bl.StatusText = EnumHelper.GetFriendlyText(bl.Status, ",");

        //            dl.InternalBatteryLogs.Add(bl);
        //        }
        //    }

        //    return dl;
        //}

        //public static BatteryStates SetInternalBatteryLogStatus(int bay, Batteries batteriesPoweringSystem, Batteries batteriesCharging)
        //{
        //    var state = BatteryStates.Unknown;

        //    // Internal battery log does not have LTC data, so we partially obtain same info as for agent battery log (except idle (plugged in).
        //    switch (bay)
        //    {
        //        case 1:
        //            if ((batteriesCharging & Batteries.BatteryOne) == Batteries.BatteryOne)
        //                state = BatteryStates.Charging;
        //            else if ((batteriesPoweringSystem & Batteries.BatteryOne) == Batteries.BatteryOne)
        //                state = BatteryStates.PoweringSystem;
        //            else
        //                state = BatteryStates.Idle;
        //            break;
        //        case 2:
        //            if ((batteriesCharging & Batteries.BatteryTwo) == Batteries.BatteryTwo)
        //                state = BatteryStates.Charging;
        //            else if ((batteriesPoweringSystem & Batteries.BatteryTwo) == Batteries.BatteryTwo)
        //                state = BatteryStates.PoweringSystem;
        //            else
        //                state = BatteryStates.Idle;
        //            break;
        //        case 3:
        //            if ((batteriesCharging & Batteries.BatteryThree) == Batteries.BatteryThree)
        //                state = BatteryStates.Charging;
        //            else if ((batteriesPoweringSystem & Batteries.BatteryThree) == Batteries.BatteryThree)
        //                state = BatteryStates.PoweringSystem;
        //            else
        //                state = BatteryStates.Idle;
        //            break;
        //        case 4:
        //            if ((batteriesCharging & Batteries.BatteryFour) == Batteries.BatteryFour)
        //                state = BatteryStates.Charging;
        //            else if ((batteriesPoweringSystem & Batteries.BatteryFour) == Batteries.BatteryFour)
        //                state = BatteryStates.PoweringSystem;
        //            else
        //                state = BatteryStates.Idle;
        //            break;
        //        case 5:
        //            if ((batteriesCharging & Batteries.BatteryFive) == Batteries.BatteryFive)
        //                state = BatteryStates.Charging;
        //            else if ((batteriesPoweringSystem & Batteries.BatteryFive) == Batteries.BatteryFive)
        //                state = BatteryStates.PoweringSystem;
        //            else
        //                state = BatteryStates.Idle;
        //            break;
        //    }

        //    return state;
        //}

        public static long DateTimeToUnixTime(DateTime date)
        {
            // Convert DateTime to time_t. https://en.wikipedia.org/wiki/C_date_and_time_functions#time_t 
            DateTime unixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            TimeSpan timeSpan = date - unixStartTime;

            return Convert.ToInt64(timeSpan.TotalSeconds);
        }

        public static DateTime UnixTimeToDateTime(long date)
        {
            // Convert time_t to DateTime. https://en.wikipedia.org/wiki/C_date_and_time_functions#time_t 
            DateTime unixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            return unixStartTime.AddSeconds(Convert.ToDouble(date)).ToUniversalTime();
        }

        public static void Uint32ToUint16(long value ,ref UInt16 hi ,ref UInt16 low)
        {
            var uint32 = (UInt32)value;
            hi = (UInt16)(uint32 >> 16);
            low = (UInt16)(uint32 & 0x0000FFFF);
        }

        public static long Uint16ToUint32(UInt16 hi,UInt16 low)
        {
            UInt32 result = ((UInt32)hi << 16) | low;
            return result;
        }
    }
}
