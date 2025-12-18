using Geni_View_SettingTool.Common;
using Geni_View_SettingTool.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Geni_View_SettingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _settingFile = @"./";

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        //public Dashboard dashboard = new Dashboard();

        public MainWindow()
        {
            try
            {
                _logger.Info($"Application Startup");

                InitializeComponent();

                Load();

                // Device device = new Device()
                // {
                //     SN = DateTime.Now.Second.ToString(),
                //     Broker = Global._setting.BrokerIP,
                //     SSID = Global._setting.LocalSSID,
                //     Status = DateTime.Now.Millisecond.ToString(),
                //     Result = ""
                // };

                //Global._dashboard.Devices.Add(device);

                DataGrid.DataContext = Global._dashboard;
                MQTTStatus.DataContext = Global._appViewModel;

                ((appViewModel)MQTTStatus.DataContext).ConnectionStatusChanged += CheckUI;

                CheckUI(this,false);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save()
        {
            var portCheck = int.TryParse(Port.Text, out int  port);
            
            Global._setting.ClientName          = ClientName.Text;
            Global._setting.BrokerIP            = IP.Text;
            Global._setting.BrokerPort          = port;
            //Global._setting.Account             = Account.Text;
            //Global._setting.Password            = Password.Text;
            Global._setting.LocalSSID           = LocalWifiSSID.Text;
            Global._setting.LocalWifiPassword   = LocalWifiPassword.Text;
            Global._setting.LocalBroker         = LocalBrokerIP.Text;
            //Global._setting.LocalBrokerAccount  = LocalBrokerAccount.Text;
            //Global._setting.LocalBrokerPassword = LocalBrokerPassword.Text;

            Global._setting.Save(_settingFile);
        }

        private void Load()
        {
            Global._setting.Read(_settingFile);

            ClientName.Text          = Global._setting.ClientName;
            IP.Text                  = Global._setting.BrokerIP;
            Port.Text                = Global._setting.BrokerPort.ToString();
            //Account.Text             = Global._setting.Account;
            //Password.Text            = Global._setting.Password;
            LocalWifiSSID.Text            = Global._setting.LocalSSID;
            LocalWifiPassword.Text        = Global._setting.LocalWifiPassword;
            LocalBrokerIP.Text       = Global._setting.LocalBroker;
            //LocalBrokerAccount.Text  = Global._setting.LocalBrokerAccount;
            //LocalBrokerPassword.Text = Global._setting.LocalBrokerPassword;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _logger.Info($"Application Closed");

        }

        private async void Connectbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Save();

                List<string> topics = new List<string>()
                {
                    Global._setting.Battery.status + "#",
                    Global._setting.Battery.wifi,
                    Global._setting.Battery.wifiResult + "#"
                };

                Global._mQTTHelper = new MQTTHelper();

                Global._mQTTHelper.Setting(
                    Global._setting.BrokerIP,
                    Global._setting.BrokerPort,
                    Global._setting.ClientName,
                    Global._setting.LocalBrokerAccount,
                    Global._setting.LocalBrokerPassword,
                    topics
                    );


                var ret = Global._mQTTHelper.Connect();

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ret = Global._mQTTHelper.Disconnect();
                Global._devices.Clear();
                Global._dashboard.AddAndClear(new List<Device>());

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Set_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckLocalSetting() == true)
                {
                    Save();

                    SendCommand();
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectALL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGrid.SelectAll();
                DataGrid.UpdateLayout();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancleALL_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                DataGrid.UnselectAll();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Global._devices.Clear();
            Global._dashboard.AddAndClear(new List<Device>());
        }

        private void CmdTest_Click(object sender, RoutedEventArgs e)
        {
            

            try
            {
                TestSendCommand();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResultTest_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                TestResult();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearTest_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                TestClear();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportList_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.DefaultExt = ".csv"; 
                openFileDialog.Filter = "CSV (.csv)|*.csv|All files (*.*)|*.*"; 
                openFileDialog.Title = "Select file"; 

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    string filename = openFileDialog.FileName; 

                    CSVHelper cSVHelper = new CSVHelper();

                    var str = cSVHelper.Read(filename);


                    foreach (var item in str)
                    {
                        Device device = new Device()
                        {
                            SN = item
                        };


                        if (Global._devices.ContainsKey(item))
                        {
                            Global._devices[device.SN] = device;
                        }
                        else
                        {
                            Global._devices.TryAdd(device.SN, device);
                        }

                    }

                    Global._dashboard.AddAndClear(Global._devices.Values.OrderBy(x => x.SN).ToList());

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.CollectInnerException(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            
        }

        private void CheckUI(object sender, bool isConnected)
        {

            Application.Current.Dispatcher.BeginInvoke(new Action(() => { 
                ImportList.IsEnabled = isConnected;
                SelectALL.IsEnabled  = isConnected;
                CancleALL.IsEnabled  = isConnected;
                Set.IsEnabled        = isConnected;
                CmdTest.IsEnabled    = isConnected;
                ResultTest.IsEnabled = isConnected;
                ClearTest.IsEnabled  = isConnected;
                Clear.IsEnabled      = isConnected;

                Connectbtn.IsEnabled = !isConnected;
                Disconnect.IsEnabled = isConnected;
            }));
        }

        private bool CheckLocalSetting()
        {
            bool result = true;

            if (
               LocalWifiSSID.Text.Length            == 0
               || LocalWifiPassword.Text.Length        == 0
               || LocalBrokerIP.Text.Length       == 0
                //||  LocalBrokerAccount.Text.Length  == 0
                //|| LocalBrokerPassword.Text.Length == 0 
                )
            {

                var ret = MessageBox.Show("A setting is empty. Confirm to continue?", "Warring", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (ret == MessageBoxResult.No)
                {
                    result = false;
                }
            }


            return result;
        }

        private void SendCommand()
        {
            var items = DataGrid.SelectedItems.OfType<Device>().ToList();

            Task.Run(async () =>
            {

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        string topic = MQTTTopic.GetLocalSetting(item.SN);

                        LocalSetting setting = new LocalSetting()
                        {
                            ID            = item.SN,
                            Cmd           = "LocalSetting",
                            SSID          = Global._setting.LocalSSID,
                            PWD           = Global._setting.LocalWifiPassword,
                            Broker        = Global._setting.LocalBroker,
                            BrokerAccount = Global._setting.LocalBrokerAccount,
                            BrokerPWD     = Global._setting.LocalBrokerPassword,
                        };
                        string data = JsonConvert.SerializeObject(setting);

                        await Global._mQTTHelper.PublishAsync(topic, data);

                        //var setting = new CommandResult()
                        //{
                        //    ID = item.SN,
                        //    Cmd = "LocalSetting",
                        //    SSID = Global._setting.LocalSSID,
                        //    PWD = Global._setting.Password,
                        //    Broker = Global._setting.BrokerIP,
                        //    Result = true
                        //};

                        //string data = JsonConvert.SerializeObject(setting);


                        //await Global._mQTTHelper.PublishAsync($"battery/wifi/result/{item.SN}", data);

                        Thread.Sleep(50);

                    }
                }

            });
        }

        private void TestSendCommand()
        {
            Task.Run(async () => {
                int count = 4;

                TestLocalSettingAsync(count);
                TestOTA(count);
                TestDeviceStatus(count);
                TestNTP(count);
                TestLogRate(count);
                TestParameter(count);

            });
        }

        private void TestResult()
        {
            Task.Run(async () => {
                int count = 100;

                //for (int i = 1; i <= 10; i++)
                //{

                //    var setting = new LocalSettingResult()
                //    {
                //        ID = i.ToString("0000"),
                //        Cmd = "LocalSetting",
                //        Result = true,
                //        SSID = Global._setting.LocalSSID,
                //        PWD = Global._setting.Password,
                //        Broker = Global._setting.BrokerIP,
                //        BrokerAccount = Global._setting.LocalBrokerAccount,
                //        BrokerPWD = Global._setting.LocalBrokerPassword,
                //    };

                //    string data = JsonConvert.SerializeObject(setting);

                //    await Global._mQTTHelper.PublishAsync($"battery/localsetting/result/{setting.ID}", data);
                //    Thread.Sleep(50);
                //}

                TestLocalSettingResult(count);
                TestOTAResult(count);
                TestNTPResult(count);
                TestLogRateResult(count);
                TestParameterResult(count);


            });
        }

        private void TestClear()
        {
            //Clear
            Task.Run(async () => {
                for (int i = 1; i < 100; i++)
                {

                    var setting = new LocalSettingResult()
                    {
                        ID = i.ToString("0000"),
                    };

                    await Global._mQTTHelper.PublishAsync($"battery/localsetting/result/{setting.ID}", "");

                    await Global._mQTTHelper.PublishAsync($"battery/localsetting/cmd/{setting.ID}", "");

                    Thread.Sleep(50);
                }

                int count = 4;


                TestLocalSettingAsync(count,true);
                TestOTA(count, true);
                TestDeviceStatus(count, true);
                TestNTP(count, true);
                TestLogRate(count, true);
                TestParameter(count, true);

                TestLocalSettingResult(count, true);
                TestOTAResult(count, true);
                TestNTPResult(count, true);
                TestLogRateResult(count, true);
                TestParameterResult(count, true);

            });
        }

        //public void DisplayMQTTStatus(bool status)
        //{
        //    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (status == true)
        //        {
        //            MQTTStatus.Text = "Online";
        //            MQTTStatus.Background = new SolidColorBrush(Colors.GreenYellow);
        //        }
        //        else
        //        {
        //            MQTTStatus.Text = "Offline";
        //            MQTTStatus.Background = new SolidColorBrush(Colors.Red);
        //        }

        //    }));


        //}

        //private Task CheckConnect()
        //{
        //    var ret = Task.Run(() => { 
            
        //        DisplayMQTTStatus(Global._mQTTHelper._connectStatus);

        //    });

        //    return ret;
        //}

        private async Task TestLocalSettingAsync(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {

                var setting = new LocalSetting()
                {
                    ID            = i.ToString("0000"),
                    Cmd           = "LocalSetting",
                    SSID          = Global._setting.LocalSSID,
                    PWD           = Global._setting.Password,
                    Broker        = Global._setting.BrokerIP,
                    BrokerAccount = Global._setting.LocalBrokerAccount,
                    BrokerPWD     = Global._setting.LocalBrokerPassword,
                };

                string data = JsonConvert.SerializeObject(setting);

                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"battery/localsetting/cmd/{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"battery/localsetting/cmd/{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestOTA(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var setting = new OTA()
                {
                    ID = i.ToString("0000"),
                    Cmd = "OTA",
                    URL = "http://192.168.10.222/Files/Device/displayboard.bin"
                };

                if (clear == false)
                {
                    string data = JsonConvert.SerializeObject(setting);

                    await Global._mQTTHelper.PublishAsync($"battery/ota/cmd/{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"battery/ota/cmd/{setting.ID}", "");
                }


                Thread.Sleep(50);
            }
        }

        private async Task TestDeviceStatus(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {

                var setting = new DeviceStatus()
                {
                    ID = i.ToString("0000"),
                    Connection = "online",
                    Type = "Battery",
                };

                string data = JsonConvert.SerializeObject(setting);


                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"device/status/{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"device/status/{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestNTP(int count,bool clear = false)
        {
            bool ntpSwitch = false;

            for (int i = 1; i <= count; i++)
            {

                var setting = new NTP();

                if (ntpSwitch == true)
                {
                    setting = new NTP()
                    {
                        ID = i.ToString("0000"),
                        Cmd = "NTP",
                        NTPURL = "time.windows.com",
                        NTPUTC = "",
                    };
                }
                else
                {
                    setting = new NTP()
                    {
                        ID = i.ToString("0000"),
                        Cmd = "NTP",
                        NTPURL = "",
                        NTPUTC = DateTime.UtcNow.ToString("o")
                    };
                }

                ntpSwitch = !ntpSwitch;


                string data = JsonConvert.SerializeObject(setting);

                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"battery/ntp/cmd/{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"battery/ntp/cmd/{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestLogRate(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {

                var setting = new LogRate()
                {
                    ID = i.ToString("0000"),
                    Cmd= "LogRate",
                    IntervalSec =300
                };

                string data = JsonConvert.SerializeObject(setting);


                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"battery/lograte/cmd/{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"battery/lograte/cmd/{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestParameter(int count, bool clear = false)
        {
            for (int i = 1; i <= count; i++)
            {
                var config = new BatteryConfig()
                {
                    ChargingMode = "Parallel",
                    DischargingMode = "Sequential",
                    AlertSettings = new Alertsettings()
                    {
                        AlertType = "All",
                        DisplayMode = "Default", 
                        SystemMode = "Disabled",
                        LowBatteryAlertInterval ="",
                        LowBatteryAlertLevel ="",
                    }
                };


                var setting = new BatterySetting()
                {
                    ID = i.ToString("0000"),
                    Cmd = "BatteryPara",
                    BatteryConfig = config
                };

                string data = JsonConvert.SerializeObject(setting);


                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"battery/para/cmd/{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"battery/para/cmd/{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestLocalSettingResult(int count, bool clear = false)
        {
            string topic = "battery/localsetting/result/";

            for (int i = 1; i <= count; i++)
            {
                var setting = new LocalSettingResult()
                {
                    ID = i.ToString("0000"),
                    Cmd = "LocalSettingResult",
                    Result = true,
                    SSID = Global._setting.LocalSSID,
                    PWD = Global._setting.Password,
                    Broker = Global._setting.BrokerIP,
                    BrokerAccount = Global._setting.LocalBrokerAccount,
                    BrokerPWD = Global._setting.LocalBrokerPassword,
                };

                string data = JsonConvert.SerializeObject(setting);


                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestOTAResult(int count, bool clear = false)
        {
            string topic = "battery/ota/result/";

            for (uint i = 2156593311; i <= 2156593311+count; i++)
            {
                var setting = new OTAResult()
                {
                    ID = i.ToString("0000"),
                    Cmd = "OTAResult",
                    Result = true,
                    URL = "http://192.168.10.222/Files/Device/displayboard.bin"
                };

                string data = JsonConvert.SerializeObject(setting);


                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestNTPResult(int count, bool clear = false)
        {
            string topic = "battery/ntp/result/";
            bool ntpSwitch = false;


            for (int i = 1; i <= count; i++)
            {
                var setting = new NTPResult();

                if (ntpSwitch == true)
                {
                    setting = new NTPResult()
                    {
                        ID = i.ToString("0000"),
                        Cmd = "NTPResult",
                        Result = true,
                        NTPURL = "time.windows.com",
                        NTPUTC = "",
                    };
                }
                else
                {
                    setting = new NTPResult()
                    {
                        ID = i.ToString("0000"),
                        Cmd = "NTP",
                        Result = true,
                        NTPURL = "",
                        NTPUTC = DateTime.UtcNow.ToString("o")
                    };
                }

                ntpSwitch = !ntpSwitch;


                string data = JsonConvert.SerializeObject(setting);


                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestLogRateResult(int count, bool clear = false)
        {
            string topic = "battery/lograte/result/";

            for (int i = 1; i <= count; i++)
            {
                var setting = new LogRateResult()
                {
                    ID = i.ToString("0000"),
                    Cmd = "LogRateResult",
                    Result = true,
                    IntervalSec = 300
                };

                string data = JsonConvert.SerializeObject(setting);


                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }

        private async Task TestParameterResult(int count, bool clear = false)
        {
            string topic = "battery/para/result/";

            for (int i = 1; i <= count; i++)
            {
                var config = new BatteryConfig()
                {
                    ChargingMode = "Parallel",
                    DischargingMode = "Sequential",
                    AlertSettings = new Alertsettings()
                    {
                        AlertType = "All",
                        DisplayMode = "Default",
                        SystemMode = "Disabled",
                        LowBatteryAlertInterval = "",
                        LowBatteryAlertLevel = "",
                    }
                };

                var setting = new BatterySettingResult()
                {
                    ID = i.ToString("0000"),
                    Cmd = "BatteryParaResult",
                    Result = true,
                    BatteryConfig = config
                };

                string data = JsonConvert.SerializeObject(setting);


                if (clear == false)
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", data);
                }
                else
                {
                    await Global._mQTTHelper.PublishAsync($"{topic}{setting.ID}", "");
                }

                Thread.Sleep(50);
            }
        }


    }
}
