using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Geni_View_SettingTool.Common;

namespace BatteryClient
{
    //Json format config file
    public class Setting
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public string ClientName            = $"SettingTool";
        public string BrokerIP              = "192.168.0.101";
        public int BrokerPort               = 1883;
        public string Account               = "";
        public string Password              = "";
        public string LocalSSID             = "LocalWiFi-G3";
        public string LocalWifiPassword     = "LocalWiFi-G3";
        public string LocalBroker           = "mqtt://192.168.10.222:1883";
        public string LocalBrokerAccount    = "";
        public string LocalBrokerPassword   = "";

        public BatteryTopic Battery = new BatteryTopic();

        public Setting()
        {

        }

        public bool Save(string path)
        {
            bool result = false;

            try
            {
                string json = JsonConvert.SerializeObject(this,Formatting.Indented);
                //Uri uri = new Uri(brokerIP);
                Directory.CreateDirectory(path);

                using (StreamWriter sw = new StreamWriter($"{path}setting.txt"))
                {
                    sw.Write(json);
                }
            }
            catch (Exception ex)
            {
                _logger.Info($"Loading setting fales. {ex.ToString()}");
            }

            return result;
        }

        public bool Read(string path)
        {
            bool result = false;

            try
            {
                using (StreamReader sr = new StreamReader($"{path}setting.txt"))
                {
                    string data = sr.ReadToEnd();
                    var ret = JsonConvert.DeserializeObject<Setting>(data);
                    ClientName          = ret.ClientName;
                    BrokerIP            = ret.BrokerIP;
                    BrokerPort          = ret.BrokerPort;
                    Account             = ret.Account;
                    Password            = ret.Password;
                    LocalSSID           = ret.LocalSSID;
                    LocalWifiPassword   = ret.LocalWifiPassword;
                    LocalBroker         = ret.LocalBroker;
                    LocalBrokerAccount  = ret.LocalBrokerAccount;
                    LocalBrokerPassword = ret.LocalBrokerPassword;

                    Battery = ret.Battery;
                }

                result = true;
            }
            catch (Exception ex)
            {
                _logger.Info($"Loading setting fales. {ex.ToString()}");

            }

            return result;
        }
    }

    public class BatteryTopic
    {
        //public string log    = "battery/log/";
        public string status     = "device/status/";
        public string wifi       = "battery/localsetting/cmd/";
        public string wifiResult = "battery/localsetting/result/";

        //public string ntp     = "battery/ntp/";


        public BatteryTopic()
        {

        }

    }
}
