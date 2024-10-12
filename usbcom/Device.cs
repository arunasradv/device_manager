using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace usbcom
{
    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }

            if (info == "")
            {

            }
        }

        private Image _imageGreyscale;
        public Image ImageGreyscale
        {
            get
            {
                return _imageGreyscale;
            }
            set
            {
                _imageGreyscale = value;
                NotifyPropertyChanged("ImageGreyscale");
            }
        }

        public Comunication ComunicationClass { set; get; }


        private string _DeviceID;
        public string DeviceID
        {
            get { return _DeviceID; }
            set
            {
                _DeviceID = value;
                NotifyPropertyChanged ("DeviceID");
            }
        }

        private int _ReadPeriod;
        public int ReadPeriod
        {
            get { return _ReadPeriod; }
            set
            {
                _ReadPeriod = value;
                NotifyPropertyChanged ("ReadPeriod");
            }
        }

        private DateTime _TimeUntilNextConnection;
        public DateTime TimeUntilNextConnection
        {
            get { return _TimeUntilNextConnection; }
            set
            {
                _TimeUntilNextConnection = value;
                NotifyPropertyChanged ("TimeUntilNextConnection");
            }
        }

        private LogItems _Logs;
        public LogItems Logs
        {
            get { return _Logs; }
            set
            {
                _Logs = value;
                NotifyPropertyChanged("Logs");
            }
        }
        public Device()
        {
            Logs = new LogItems();
            ComunicationClass = new Comunication();
            ComunicationClass.SoftwareBuffer.PropertyChanged += ComunicationClass_PropertyChanged;
            ImageGreyscale = new Image ( );
        }

        private string _ReceivedBytes;
        public string ReceivedBytes
        {
            get { return _ReceivedBytes; }
            set
            {
                _ReceivedBytes = value;
                NotifyPropertyChanged("ReceivedBytes");
            }
        }

        private byte [] parse_buff = new byte [1024];
        private int parse_index = 0;
        private byte [] parse_buff_r = new byte [1024];
        private int parse_index_r = 0;

        private void ComunicationClass_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {           
            if (e.PropertyName.ToString() == "InBuffHead"/*"AllReceived"*/)
            {
                byte[] dataIn = null;
                int rBytes = Convert.ToInt32(ReceivedBytes);

                int recBytyes = ComunicationClass.SoftwareBuffer.InBuffHead >= ComunicationClass.SoftwareBuffer.InBuffTail ?
                    ComunicationClass.SoftwareBuffer.InBuffHead-ComunicationClass.SoftwareBuffer.InBuffTail :
                    ComunicationClass.SoftwareBuffer.InBuff.Length -ComunicationClass.SoftwareBuffer.InBuffTail+ComunicationClass.SoftwareBuffer.InBuffHead;
                              
             
                byte [] databyte = ComunicationClass.SoftwareBuffer.GetNextByteFromInBuff();
                
                if(databyte!=null)
                {

                    #region ASCII PARSER             
                    parse_buff_r [parse_index_r] = databyte [0];

                    if(parse_buff_r [parse_index_r] == '\r')
                    {
                        byte[] truncArray = new byte[parse_index_r];

                        Array.Copy (parse_buff_r ,truncArray ,parse_index_r);
                        AddLog (LogItems.GetLogLine (truncArray ,Logs.Hex) ,Colors.DarkGray ,"Device:");
                        parse_index_r = 0;
                    }
                    else
                    {
                        parse_index_r++;
                    }
                    #endregion

                    #region LED PARSER
                    parse_buff [parse_index] = databyte[0];
                    if(databyte [0]==0xAE) // end?
                    {
                        com_protocol.apply_receive_patch(parse_buff,1,parse_index-2,out byte [] data_patched);

                        ushort crc = com_protocol.crc(parse_buff,1,parse_index-3);

                       

                        if(crc!=(parse_buff [parse_index-2]|parse_buff [parse_index-1]<<8))
                        {
                            AddLog(LogItems.GetLogLine(data_patched,Logs.Hex),Colors.Red,"Device: crc error");
                        }
                        else
                        {
                            if(parse_buff[5] !=0 )
                            {
                                AddLog(LogItems.GetLogLine(data_patched,Logs.Hex),Colors.Red,$"Device: error {parse_buff[5]}");
                            } 
                            else
                            {
                                AddLog(LogItems.GetLogLine(data_patched,Logs.Hex),Colors.Green,"Device:");
                            }
                        }
                       

                        parse_index=0;
                        
                        ReceivedBytes =rBytes.ToString();
                    }
                    else
                    {
                       
                        parse_index++;
                    }

                    #endregion
                }
            }
        }

        public void AddLog(string s, Color color, string dir)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                tLogItem li = new tLogItem();
                li.LogDirection = dir;
                li.LogString = s;
                li.LogTimeString = DateTime.Now.ToString("<yyyy-MM-dd, HH:mm:ss.fff>");
                li.LogColor = new SolidColorBrush(color);
                if (Logs.Items.Count > 1000)
                    Logs.Items.RemoveAt(Logs.Items.Count - 1);
                Logs.Items.Insert(0, li);
            });
        }

        public Image CreateImage (byte [] imageData,int width,int height, int bytes_per_pixel)
        {
            if(imageData == null || imageData.Length == 0)
                return null;           
            PixelFormat pf = bytes_per_pixel == 2 ? PixelFormats.Gray16 : bytes_per_pixel == 3 ? PixelFormats.Bgr24 : PixelFormats.Bgr32;          
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];
            // Initialize the image with data.
            Random value = new Random();
            value.NextBytes (rawImage);
            // Create a BitmapSource.
            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, pf, null,  rawImage, rawStride);
            // Create an image element;
            Image image = new Image();
            image.Width = width;
            // Set image source.
            image.Source = bitmap;
            return image;
        }

        public void CreateRandomImage ()
        {
            byte[] image_bytes = new byte[320*240*3];
             this.ImageGreyscale.Source = this.CreateImage (image_bytes ,320 ,240, 3).Source;
        }

    }
}
