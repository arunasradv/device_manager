using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
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
using static System.Net.Mime.MediaTypeNames;

namespace usbcom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DeviceList Devices = new DeviceList();
        int DeviceIndex;

        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        void init()
        {
            AddDeviceToList();
            ServerDevicesListBox.ItemsSource = Devices.Items;


        }

        private void ServerDevicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = ((ListBox)sender).SelectedIndex;
            ChangeSources(i);
        }

        public void ChangeSources(int i)
        {
            if ((i >= 0) && (i < Devices.Items.Count))
            {
               LoggroupBox.DataContext = Devices.Items[i].device.Logs;               
               DeviceIndex = i;
            }
        }

        private void ConnectViaTCPButton_Click(object sender, RoutedEventArgs e)
        {
            if (Devices.Items[DeviceIndex].device.ComunicationClass.SelectedComInterface == Comunication._pipe_t.pi_usb)
            {
                Devices.Items[DeviceIndex].device.ComunicationClass.TryDisconnect();
            }

            Devices.Items[DeviceIndex].device.ComunicationClass.SelectedComInterface = Comunication._pipe_t.pi_modem_tcp;
            if (Devices.Items[DeviceIndex].device.ComunicationClass.tcpClientSocket != null)
            {
                if (Devices.Items[DeviceIndex].device.ComunicationClass.tcpClientSocket.Connected == true)
                {
                    Devices.Items[DeviceIndex].device.ComunicationClass.TryDisconnect();
                }
                else
                {
                    Devices.Items[DeviceIndex].device.ComunicationClass.TryConnect();
                }
            }
            else
            {
                Devices.Items[DeviceIndex].device.ComunicationClass.TryConnect();
            }
        }

        private void ConnectViaUSBButton_Click(object sender, RoutedEventArgs e)
        {
            if (Devices.Items[DeviceIndex].device.ComunicationClass.SelectedComInterface == Comunication._pipe_t.pi_modem_tcp)
            {
                Devices.Items[DeviceIndex].device.ComunicationClass.TryDisconnect();
            }

            Devices.Items[DeviceIndex].device.ComunicationClass.SelectedComInterface = Comunication._pipe_t.pi_usb;
            if (Devices.Items[DeviceIndex].device.ComunicationClass.serialPort != null)
            {
                if (Devices.Items[DeviceIndex].device.ComunicationClass.serialPort.IsOpen)
                {
                    Devices.Items[DeviceIndex].device.ComunicationClass.TryDisconnect();
                }
                else
                {
                    Devices.Items[DeviceIndex].device.ComunicationClass.TryConnect();
                }
            }
            else
            {
                Devices.Items[DeviceIndex].device.ComunicationClass.TryConnect();
            }
        }

        private void GroupBox_MouseEnter(object sender, MouseEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                string[] coms = SerialPort.GetPortNames();
                if (Devices.Items != null)
                {
                    if (Devices.Items.Count > 0)
                    {
                        for (int i = 0; i < Devices.Items.Count; i++)
                        {
                            if (coms.Length != Devices.Items[i].COMArray.Length)
                                Devices.Items[i].COMArray = coms;
                        }
                        //if (Convert.ToInt64(DevicesList.Items[DeviceIndex].Device.ReceivedBytes) > 0)
                        //{
                        //    DevicesList.Items[DeviceIndex].Device.ReceivedBytes = "0";
                        //}
                    }
                }
            });
        }

        private void AddDeviceToList()
        {
            DeviceListItem item = new DeviceListItem();
            Devices.Items.Add(item);
            ServerDevicesListBox.DataContext = Devices;
            // Devices.Items[Devices.Items.Count - 1].PropertyChanged += Devices_PropertyChanged;
            // Devices.Items[Devices.Items.Count - 1].device.ModemStatus.PropertyChanged += ModemStatus_PropertyChanged;
            // Devices.Items[Devices.Items.Count - 1].device.Status.PropertyChanged += Status_PropertyChanged;
            Devices.Items [Devices.Items.Count - 1].device.DeviceID = "0";
            Devices.Items[Devices.Items.Count - 1].Enable = false;
            ServerDevicesListBox.SelectedIndex = Devices.Items.Count - 1;
        }

        private void SendFile_btn_Click(object sender,RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
         
            if(ofd.ShowDialog() == true)
            {
                StreamReader sr = new StreamReader(ofd.FileName);
                List<byte> data = new List<byte>();
               
                do
                {
                    int d =  sr.Read(); 
                  
                    if(d != -1)
                    {
                        data.Add((byte)(d&0xFF));
                    }
                    
                }while (sr.EndOfStream);

                byte [] file_data = data.ToArray();
               // byte [] outdata = com_protocol.MakeCommandPacket((byte) 0x01,(byte)0x00,(byte)0x00, file_data);

               // Devices.Items [ServerDevicesListBox.SelectedIndex].device.ComunicationClass.SoftwareBuffer.AddToOutBuff(outdata, outdata.Length);
            }
        }

        private void SendStaticText_btn_Click(object sender,RoutedEventArgs e)
        {/*
            byte cc = 0x04;

            byte [] text = Encoding.ASCII.GetBytes(tb_MessageText.Text);

            byte [] cc_and_data = com_protocol.make_cc_static_text(
                cc,
                Convert.ToByte(cb_WindowNo.SelectedIndex), // window
                Convert.ToByte(cb_Aligment.SelectedIndex), // alligment
                0, // left x
                0, // left y
                64, // width
                16, //heigth
                Convert.ToByte(cb_FontSize.SelectedIndex), // font size
                Convert.ToByte(cb_FontStyle.SelectedIndex), // font style
                Convert.ToByte(cb_RedColor.SelectedIndex),
                Convert.ToByte(cb_GreenColor.SelectedIndex),
                Convert.ToByte(cb_BlueColor.SelectedIndex),
                text);

            byte [] outdata = com_protocol.MakePackets(1,1,0,512,cc,cc_and_data);

            Devices.Items [ServerDevicesListBox.SelectedIndex].device.ComunicationClass.SoftwareBuffer.AddToOutBuff(outdata, outdata.Length);*/
        }

        private void TerminalSendButton_Click (object sender ,RoutedEventArgs e)
        {
            if(TerminalBox.Text.Length > 0)
            {
                List<byte> bytes = new List<byte>();

                string ready_string = string.Concat (TerminalBox.Text ,'\r');

                byte[] outdata = Encoding.UTF8.GetBytes(ready_string);

                Devices.Items [ServerDevicesListBox.SelectedIndex].device.ComunicationClass.SoftwareBuffer.AddToOutBuff(outdata ,outdata.Length);
            }
        }

        private void btLoadImage_Click (object sender ,RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = ""; // Default file name
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "BMP Image (.bmp)|*.bmp"; // Filter files by extension

            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if(result == true)
            {
                Devices.Items[DeviceIndex].device.ImageGreyscale.Source = new BitmapImage( new Uri(dialog.FileName));

               
            }
        }

        private void btCreateImage_Click (object sender ,RoutedEventArgs e)
        {
            Devices.Items [DeviceIndex].device.CreateRandomImage ( );
        }


        
    }
}
