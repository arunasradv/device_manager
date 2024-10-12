using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace usbcom
{
    public class DeviceListItem : INotifyPropertyChanged
    {
        private bool _Enable;
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                _Enable = value;
                NotifyPropertyChanged("Enable");
            }
        }

        private string[] _COMArray;
        public string[] COMArray
        {
            get { return _COMArray; }
            set
            {
                _COMArray = value;
                NotifyPropertyChanged("COMArray");
            }
        }

        private Device _device;
        public Device device
        {
            get { return _device; }
            set
            {
                _device = value;
                NotifyPropertyChanged("Loger");
            }
        }

        private bool _Active;
        public bool Active
        {
            get { return _Active; }
            set
            {
                _Active = value;
                NotifyPropertyChanged("Active");
            }
        }

        private string _COMactive;
        public string COMactive
        {
            get { return _COMactive; }
            set
            {
                _COMactive = value;
                NotifyPropertyChanged("COMactive");
            }
        }

        private string _TCPactive;
        public string TCPactive
        {
            get { return _TCPactive; }
            set
            {
                _TCPactive = value;
                NotifyPropertyChanged("TCPactive");
            }
        }

        private bool _EnableRealTime;
        public bool EnableRealTime
        {
            get { return _EnableRealTime; }
            set
            {
                _EnableRealTime = value;
                NotifyPropertyChanged("EnableRealTime");
            }
        }

        public DeviceListItem()
        {
            COMactive = "Connect";
            TCPactive = "Connect";
            Enable = false;
            COMArray = new string[0];
            device = new Device();
            Process();
        }

        private void Process()
        {
            Task.Run(async () =>
            {
                while (this != null)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            device.ComunicationClass.SendPacket();
#warning no process
                            //bool enable = device.ClientProcess(_Enable);
                            //if (Enable != enable)
                            //{
                            //    Enable = enable;
                            //}

                            if (device.ComunicationClass.serialPort != null)
                            {
                                if (device.ComunicationClass.serialPort.IsOpen)
                                {
                                    if (Active == false)
                                    {
                                        Active = true;
                                        COMactive = "DisConnect";
                                    }
                                }
                                else
                                {
                                    if (Active == true)
                                    {
                                        Active = false;
                                        COMactive = "Connect";
                                    }
                                }
                            }

                            if (device.ComunicationClass.tcpClientSocket != null)
                            {
                                if (device.ComunicationClass.tcpClientSocket.Connected)
                                {
                                    if (Active == false)
                                    {
                                        Active = true;
                                        TCPactive = "DisConnect";
                                    }

                                }
                                else
                                {
                                    if (Active == true)
                                    {
                                        Active = false;
                                        TCPactive = "Connect";
                                    }

                                }
                            }
                        });
                        await Task.Delay(100);
                    }
                    catch
                    {
                    }
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
