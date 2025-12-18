using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace GeniView.Cloud.Models
{
    public class GlobalSettings
    {
        private static T Setting<T>(string configName)
        {
            string value = ConfigurationManager.AppSettings[configName];

            if (value == null)
            {
                throw new Exception(String.Format("Could not find setting '{0}',", configName));
            }

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        private static void Setting(string configName, string value)
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration("/");
            if (config.AppSettings.Settings[configName] != null)
            {
                config.AppSettings.Settings[configName].Value = value;
                config.Save(ConfigurationSaveMode.Modified);
            }
        }
        
        public static DateTime OnlineRangeInMinutes
        { 
            get { return DateTime.UtcNow.AddMinutes(-1*Setting<int>("OnlineRangeInMinutes")); }
        }

        public static int OnlineRangeInMinutesValue
        {
            get { return Setting<int>("OnlineRangeInMinutes"); }
            set { Setting("OnlineRangeInMinutes", value.ToString()); }
        }

        public static DateTime OfflineRangeInDays
            {
                get { return DateTime.UtcNow.AddDays(-1*Setting<int>("OfflineRangeInDays")); }
            }
        public static int OfflineRangeInDaysValue
        {
            get { return Setting<int>("OfflineRangeInDays"); }
            set { Setting("OfflineRangeInDays", value.ToString()); }
        }

        
        public static string SuccessColor
        {
            get { return Setting<string>("SuccessColor"); }
            set { Setting("SuccessColor", value.ToString()); }
        }

        public static string WarningColor
        {
            get { return Setting<string>("WarningColor"); }
            set { Setting("WarningColor", value.ToString()); }
        }

        public static string AlertColor
        {
            get { return Setting<string>("AlertColor"); }
            set { Setting("AlertColor", value.ToString()); }
        }
            
        // Temperature Global Setting
        public static int SuccessTemperature
        {
            get { return Setting<int>("SuccessTemperature"); }
            set { Setting("SuccessTemperature", value.ToString()); }
        }

        public static int AlertTemperature
        {
            get { return Setting<int>("AlertTemperature"); }
            set { Setting("AlertTemperature", value.ToString()); }
        }

        // Temperature Global Setting
        public static int SuccessChargingLVL
        {
            get { return Setting<int>("SuccessChargingLVL"); }
            set { Setting("SuccessChargingLVL", value.ToString()); }
        }

        public static int AlertChargingLVL
        {
            get { return Setting<int>("AlertChargingLVL"); }
            set { Setting("AlertChargingLVL", value.ToString()); }
        }

        public static int IsStateOfChargeReadyToUse
        {
            get { return Setting<int>("IsStateOfChargeReadyToUse"); }
            set { Setting("IsStateOfChargeReadyToUse", value.ToString()); }
        }

        public static double NominalVoltage
        {
            get { return Setting<double>("NominalVoltage"); }
            set { Setting("NominalVoltage", value.ToString()); }
        }
        public static string BingMapKey
        {
            get { return Setting<string>("BingMapKey"); }
            set { Setting("BingMapKey", value.ToString()); }
        }
        public static int NotificationDelayTimeInSeconds
        {
            get { return Setting<int>("NotificationDelayTimeInSeconds"); }
            set { Setting("NotificationDelayTimeInSeconds", value.ToString()); }
        }

        public static int NotificationToleranceInMinutes
        {
            get { return Setting<int>("NotificationToleranceInMinutes"); }
            set { Setting("NotificationToleranceInMinutes", value.ToString()); }
        }

        public static int UserLockoutTimeInMinutes
        {
            get { return Setting<int>("UserLockoutTimeInMinutes"); }
            set { Setting("UserLockoutTimeInMinutes", value.ToString()); }
        }

        public static int ScanDeviceDurationMinutes
        {
            get { return Setting<int>("ScanDeviceDurationMinutes"); }
            set { Setting("ScanDeviceDurationMinutes", value.ToString()); }
        }
    }
    
}