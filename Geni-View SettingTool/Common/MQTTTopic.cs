using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Geni_View_SettingTool.Common
{
    public static class MQTTTopic
    {
        static public string Battery { get; set; }              = "battery";
        static public string Dock { get; set; }                 = "dock";
        static public string LogRate { get; set; }              = "lograte";

        static public string BatteryLog { get; set; } = "battery/log";
        static public string DockLog { get; set; } = "dock/log";

        static public string BatteryLogRate { get; set; }       = "battery/lograte/cmd";
        static public string BatteryLogRateResult { get; set; } = "battery/lograte/result";

        static public string BatteryWiFi { get; set; } = "battery/localsetting/cmd";
        static public string BatteryWiFiResult { get; set; } = "battery/localsetting/result";

        //Because need to identify the result topic. Use compiled regex performance almost close to split and more clearly.
        static public readonly Regex BatteryLogRegex           = new Regex(@"^battery\/log\/[^\/]+$", RegexOptions.Compiled);
        static public readonly Regex BatteryLograteRegex       = new Regex(@"^battery\/lograte\/[^\/]+$", RegexOptions.Compiled);
        static public readonly Regex BatteryLograteResultRegex = new Regex(@"^battery\/lograte\/result\/[^\/]+$", RegexOptions.Compiled);

        static public readonly Regex DockLogRegex = new Regex(@"^dock\/log\/[^\/]+$", RegexOptions.Compiled);



        static public List<string> Topics { get; set; } = new List<string>()
        {
           $"{BatteryLog}/#",
           $"{DockLog}/#",
           $"{BatteryLogRateResult}/#",
           $"{BatteryWiFiResult}/#",
        };


        public static string GetLogRate(string id)
        {
            string result = $"{BatteryLogRate}/{id}";

            return result;
        }

        public static string GetLocalSetting(string id)
        {
            string result = $"{Battery}/localsetting/cmd/{id}";

            return result;
        }


    }
}