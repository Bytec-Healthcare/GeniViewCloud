using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geni_View_SettingTool.Models
{
    public class BatteryConfig
    {
        public string Bays { get; set; }
        public string OemIdentifier { get; set; }
        public string ChargingMode { get; set; }
        public string DischargingMode { get; set; }
        public Standbysettings StandbySettings { get; set; }
        public Alertsettings AlertSettings { get; set; }
        public string BargraphDimming { get; set; }
        public Batterystateofchargesettings BatteryStateOfChargeSettings { get; set; }
        public Userinformation UserInformation { get; set; }
        public Systeminformation SystemInformation { get; set; }
        public Poweroutputsettings PowerOutputSettings { get; set; }
        public string LoggingInterval { get; set; }
    }

    public class Standbysettings
    {
        public string Current { get; set; }
        public string Time { get; set; }
        public string StandbyOnExternalPowerInput { get; set; }
    }

    public class Alertsettings
    {
        public string AlertType { get; set; }
        public string DisplayMode { get; set; }
        public string SystemMode { get; set; }
        public string LowBatteryAlertInterval { get; set; }
        public string LowBatteryAlertLevel { get; set; }
    }

    public class Batterystateofchargesettings
    {
        public string VoltageCutOff { get; set; }
        public string PercentageCutOff { get; set; }
    }

    public class Userinformation
    {
        public string Name { get; set; }
    }

    public class Systeminformation
    {
        public string SystemPartNumber { get; set; }
        public string SystemSerialNumber { get; set; }
        public string SlaveSerialNumber { get; set; }
    }

    public class Poweroutputsettings
    {
        public string Voltage { get; set; }
        public string Power { get; set; }
        public string Trim { get; set; }
        public string PotValue { get; set; }
        public string SaveToDigitalPot { get; set; }
        public string IsDCConverterFitted { get; set; }
    }

}
