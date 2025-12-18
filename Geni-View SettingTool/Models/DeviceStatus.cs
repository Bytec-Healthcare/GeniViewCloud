using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Geni_View_SettingTool.Models
{
    public class Device: INotifyPropertyChanged
    {
        private string _sn;
        public string SN 
        { 
            get { return _sn; } 
            set 
            {
                _sn = value;
                OnPropertyChanged("SN"); 
            } 
        }

        private string _ssid;
        public string SSID
        {
            get { return _ssid; }
            set
            {
                _ssid = value;
                OnPropertyChanged("SSID");
            }
        }

        private string _broker;
        public string Broker
        {
            get { return _broker; }
            set
            {
                _broker = value;
                OnPropertyChanged("Broker");
            }
        }

        private bool _result;
        public bool Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        private string _createTime;
        public string CreateTime
        {
            get { return _createTime; }
            set
            {

                DateTime dt;

                if (DateTime.TryParse(value, out dt) == true)
                {
                    _createTime = dt.ToString("yyyy-MM-dd HH:mm:ss:ffff");
                }
                else
                {
                    _createTime = value;
                }


                OnPropertyChanged("CreateTime");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class Dashboard : INotifyPropertyChanged
    {
        public ObservableCollection<Device> Devices { get; set; }

        public Dashboard()
        {
            Devices = new ObservableCollection<Device>();
        }

        public bool AddAndClear(List<Device> devices)
        {
            bool result = false;
            

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Devices.Clear();

                foreach (var item in devices)
                {
                    Devices.Add(item);
                }

            }));


            return result;
        }

        public bool AddOrUpdate(string sn ,Device data)
        {
            bool result = false;
                var device = Devices.Where(x => x.SN == sn).FirstOrDefault();

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (device == null)
                    {
                        Devices.Add(data);
                        result = true;
                    }
                    else
                    {
                        device = data;

                        result = true;
                    }
                }));

            return result;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
