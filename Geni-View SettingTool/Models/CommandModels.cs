using GeniView.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geni_View_SettingTool.Models
{
    public class Message
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public Message(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    public class DeviceStatus 
    {
        public string ID { get; set; }
        public string Cmd { get; set; }
        public string Connection { get; set; }
        public string Type { get; set; }
        public string DateTimeUTC { get; set; }
        public Guid? Guid { get; set; }
        public string Raw { get; set; }

        public DeviceStatus()
        {
            Guid = System.Guid.NewGuid();
            Cmd = "Status";
            DateTimeUTC = DateTime.UtcNow.ToString("o");
            Raw = "";
        }
    }

    public class Command
    {
        public string ID { get; set; }

        public string Cmd { get; set; }

        public Guid? Guid { get; set; }

        public string DateTimeUTC { get; set; }

        public string Raw { get; set; }

        public Command()
        {
            Guid = System.Guid.NewGuid();
            DateTimeUTC = DateTime.UtcNow.ToString("o");
            Raw = "";
        }
    }

    public class CommandResult : Command
    {
        //public int IntervalSec { get; set; }
        public string SN { get; set; }

        public bool Result { get; set; }

        public string Connection { get; set; }

        public string Type { get; set; }

        public bool Status { get; set; }

        public string SSID { get; set; }

        public string PWD { get; set; }

        public string Broker { get; set; }

        public string BrokerAccount { get; set; }

        public string BrokerPWD { get; set; }

        public CommandResult() 
        {
            if (ID != null && ID.Length >= 10)
            {
                var snCodeCheck = long.TryParse(ID, out long sncode);
                if (snCodeCheck)
                {
                    SN = DataHelpers.BatterySerialNumberCodeToSerialNumber(sncode);
                }
            }
        }
    }


    public class LogRate : Command
    {
        [JsonProperty(Order = 1)]
        public int IntervalSec { get; set; }

        public LogRate() { }
        public LogRate(int sec)
        {
            IntervalSec = sec;
        }
    }

    public class LogRateResult : LogRate
    {
        public bool Result { get; set; }
        public LogRateResult() { }
    }

    public class LocalSetting : Command
    {
        [JsonProperty(Order = 1)]
        public string SSID { get; set; }
        [JsonProperty(Order = 2)]
        public string PWD { get; set; }
        [JsonProperty(Order = 3)]
        public string Broker { get; set; }
        [JsonProperty(Order = 4)]
        public string BrokerAccount { get; set; }
        [JsonProperty(Order = 5)]
        public string BrokerPWD { get; set; }

        public LocalSetting() { }

        public LocalSetting(string ssid , string pwb ,string broker, string brokeraccount, string brokerpwd)
        {
            SSID = ssid;
            PWD = pwb;

            Broker = broker;
            BrokerAccount = brokeraccount;
            BrokerPWD = brokerpwd;
        }
    }

    public class LocalSettingResult : LocalSetting
    {
        public bool Result { get; set; }
        public LocalSettingResult() { }
    }


    public class OTA : Command
    {
        [JsonProperty(Order = 1)]
        public string URL { get; set; }

        public OTA() { }
        public OTA(string url)
        {
            URL = url;
        }
    }

    public class OTAResult : OTA
    {
        public bool Result { get; set; }

        public OTAResult() { }
    }


    public class NTP : Command
    {
        [JsonProperty(Order = 1)]
        public string NTPURL { get; set; }
        [JsonProperty(Order = 2)]
        public string NTPUTC { get; set; }

        public NTP() { }
        public NTP(string url,string utc)
        {
            NTPURL = url;
            NTPUTC = utc;
        }
    }

    public class NTPResult : NTP
    {
        public bool Result { get; set; }

        public NTPResult() { }
    }

    public class BatterySetting : Command
    {
        [JsonProperty(Order = 1)]
        public BatteryConfig BatteryConfig { get; set; }
        public BatterySetting() { }
    }

    public class BatterySettingResult : BatterySetting
    {
        public bool Result { get; set; }

        public BatterySettingResult() { }
    }
}