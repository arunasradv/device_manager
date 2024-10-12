using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace usbcom
{
    public class Comunication : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; 
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public enum stmsg : int
        {
            _stmsg_none = 0,
            _stmsg_connecting,
            _stmsg_connected,
            _stmsg_connected_start,
            _stmsg_sending,
            _stmsg_send_completed,
            _stmsg_send_timeout,
            _stmsg_receive_timeout,
            _stmsg_disconnected,
            _stmsg_connecting_timeout,
            _stmsg_read_device_version,
            _stmsg_got_device_version,
            _stmsg_finishedupdatepleasewait,
            _stmsg_updatedtonewdeviceversion,
            _stmsg_failedtoupdate,
            _stmsg_sendtrycountexpired,
            _stmsg_received,
            _stmsg_badindex,
            _stmsg_array,
            _stmsg_restartupdate,
            _stmsg_parserreceivetimeout,
            _stmsg_finished,
            _stmsg_gettingcmd,
            _stmsg_waitting,
            _stmsg_stop,
            _stmsg_download,
            _stmsg_connect_fail,
            _stmsg_portsetupfail
        }

        public enum _pipe_t
        {
            pi_none,
            pi_usb,
            pi_modem_tcp,
            pi_rs485,
            pi_j7, // I2C
            pi_j7_uart
        };


        public static string[] _pipe__String = {
            "NONE",
            "USB",
            "MODEM TCP",
            "RS485",
            "J7 I2C",
            "J7 UART"
             };

        private string[] __pipe__String;
        public string[] pipe__String
        {
            get { return __pipe__String; }
            set
            {
                __pipe__String = value;
                NotifyPropertyChanged("pipe__String");
            }
        }

        private int _AllDone;
        public int AllDone
        {
            get { return _AllDone; }
            set
            {
                _AllDone = value;
                NotifyPropertyChanged("AllDone");
            }
        }

        private int _AllSent;
        public int AllSent
        {
            get { return _AllSent; }
            set
            {
                _AllSent = value;
                NotifyPropertyChanged("AllSent");
            }
        }

        private int _AllReceived;
        public int AllReceived
        {
            get { return _AllReceived; }
            set
            {
                _AllReceived = value;
                NotifyPropertyChanged("AllReceived");
            }
        }

        public byte[] InBuff;
        private  int InLen;

        private _pipe_t _SelectedComInterface;
        public _pipe_t SelectedComInterface
        {
            get { return _SelectedComInterface; }
            set
            {
                _SelectedComInterface = value;
                NotifyPropertyChanged("SelectedComInterface");
            }
        }

        private SerialPort _SerialPort;
        public SerialPort serialPort
        {
            get { return _SerialPort; }
            set
            {
                _SerialPort = value;
                NotifyPropertyChanged("serialPort");
            }
        }

        private string _ComPort;
        public string ComPort
        {
            get { return _ComPort; }
            set
            {
                _ComPort = value;
                NotifyPropertyChanged("ComPort");
            }
        }

        private Socket _tcpClientSocket;
        public Socket tcpClientSocket
        {
            get { return _tcpClientSocket; }
            set
            {
                _tcpClientSocket = value;
                NotifyPropertyChanged("tcpClientSocket");
            }
        }

        private string _IP;
        public string IP
        {
            get { return _IP; }
            set
            {
                _IP = value;
                NotifyPropertyChanged("IP");
            }
        }

        private int _Client_Port;
        public int Client_Port
        {
            get { return _Client_Port; }
            set
            {
                _Client_Port = value;
                NotifyPropertyChanged("Client_Port");
            }
        }

        public SoftwareBuff SoftwareBuffer = new SoftwareBuff();

        private int TimeOut;
        private Timer timer0;
       
        private stmsg _StatusMessageFlag;
        public stmsg StatusMessageFlag
        {
            get { return _StatusMessageFlag; }
            set
            {
                _StatusMessageFlag = value;
                NotifyPropertyChanged("StatusMessageFlag");
            }
        }
        private stmsg _StatusMessageOldFlag;
        public stmsg StatusMessageOldFlag
        {
            get { return _StatusMessageOldFlag; }
            set
            {
                _StatusMessageOldFlag = value;
                NotifyPropertyChanged("StatusMessageOldFlag");
            }
        }
       
        private int _CmdProcessState;
        public int CmdProcessState
        {
            get { return _CmdProcessState; }
            set
            {
                _CmdProcessState = value;
                NotifyPropertyChanged("CmdProcessState");
            }
        }
      
        public Comunication()
        {
            AllDone = 1;
            AllSent = 1;
            AllReceived = 1;
            InBuff = new byte[24000];
            SelectedComInterface = _pipe_t.pi_none;
            SoftwareBuffer = new SoftwareBuff();
            SoftwareBuffer.MaxMessageLength = 60;         
            ComPort = "COM1";           
            timer0 = SetTimer(timer0, 100, true, true, Timer0_Tick);
            StatusMessageFlag = 0;
            CmdProcessState = new int();
            InLen = 0;
            _IP = "0.0.0.0";
            _Client_Port = 38000;
            StatusMessageFlag = new int();
            StatusMessageOldFlag = new int();
            ReceiveAsync();
        }

        public void ReceiveAsync()
        {
            Task.Run(async () =>
            {
                while (this != null)
                {
                    await Task.Delay(10);
                    ReceivePacket();
                }
            });
        }

        private Timer SetTimer(Timer timerX, int period, bool autoreset, bool enabled, ElapsedEventHandler ElapsedEventHandlerFunctionName)
        {
            timerX = new Timer();
            timerX.Interval = period;
            timerX.Elapsed += ElapsedEventHandlerFunctionName;
            timerX.AutoReset = autoreset;
            timerX.Enabled = enabled;
            return (timerX);
        }

        private void SetTimeOut(int timeout)
        {
            this.TimeOut = (int)((1000 * timeout) / this.timer0.Interval);
        }

        private void Timer0_Tick(object sender, EventArgs e)
        {
            if (this.TimeOut > 0)
            {
                this.TimeOut--;
            }
        }

        public void TryReConnect()
        {
            if (AllDone == 1) // neprisijungta ir jungimosi bandymas (BeginConnect) baigesi
            {
                StatusMessageFlag = stmsg._stmsg_connecting;
                //this.UpdateStatusMessageFlag = 1;
                TryConnect();
            }
        }

        /// <summary>
        ///  This function will try to connect socket to remote device. If socket is null, function will create one.
        ///  The function is using asynchronous metod to make connection.It sets tcpAllDone flag to 0. If exeption
        ///  occured tcpAllDone flag will be set to 1
        /// </summary>
        public void TryConnect()
        {
            if (this.TimeOut == 0)
            {
                if (this.SelectedComInterface == _pipe_t.pi_modem_tcp)
                {
                    try
                    {
                        if (this.tcpClientSocket != null)
                        {                           
                           // this.tcpClientSocket.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    this.tcpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    try
                    {
                        if ((this.IP != null) && (this.IP != "") && (this.IP != "0.0.0.0"))
                        {
                            AllDone = 0;
                            this.tcpClientSocket.BeginConnect(Dns.GetHostAddresses(this.IP), this.Client_Port, new AsyncCallback(this._ConnectCallback), this.tcpClientSocket);
                        }
                        else
                        {           
                           this.SelectedComInterface = _pipe_t.pi_usb;
                        }
                    }
                    catch (Exception ex)
                    {
                        AllDone = 1;
                        this.TryDisconnect();                    
                        // MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (this.SelectedComInterface == _pipe_t.pi_usb)
                {
                    try
                    {
                        if (this._SerialPort != null)
                        {
                            this._SerialPort.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    try
                    {
                        if(this.ComPort != "")
                        {
                            this.SetComPort(this.ComPort);
                        }                        
                    }
                    catch (Exception ex)
                    {
                       // MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    try
                    {
                        if (this._SerialPort != null)
                        {                            
                            this._SerialPort.Open();
                            StatusMessageFlag = stmsg._stmsg_connected;
                        }
                    }
                    catch (Exception ex)
                    {
                        AllDone = 1;
                    }
                }
                SetTimeOut(2);
            }
        }
        
        public void SetComPort(string portname)
        {
            this.ComPort = portname;
            if (this._SerialPort != null)
                TryDisconnect();
            this._SerialPort = null;
            this._SerialPort = new SerialPort(this.ComPort, 115200, Parity.None, 8, StopBits.One);
            this._SerialPort.DtrEnable = false;
        }

        public bool SendPacket()
        {
            if (this.SelectedComInterface == _pipe_t.pi_modem_tcp)
            {
                try
                {
                    if (this.tcpClientSocket != null)
                    {
                        if (this.tcpClientSocket.Connected == true)
                        {
                            SoftwareBuffer.MaxMessageLength = 1024;
                            if ((this.AllSent == 1) && (this.AllDone == 1))
                            {
                                byte[] buff = SoftwareBuffer.SuckFromOutBuff();
                                if (buff != null)
                                {
                                    StatusMessageFlag = stmsg._stmsg_sending;
                                    this.AllSent = 0;
                                    this.tcpClientSocket.BeginSend(buff, 0, buff.Length, SocketFlags.None, new AsyncCallback(_SendCallback), this.tcpClientSocket);
                                }
                                else
                                {
                                    if (StatusMessageFlag == stmsg._stmsg_sending)
                                    {
                                        StatusMessageFlag = stmsg._stmsg_send_completed;
                                    }
                                    return (true);
                                }
                            }
                        }
                        else
                        {
                            if(SoftwareBuffer.OutBuffTail != SoftwareBuffer.OutBuffHead)
                            {
                                TryReConnect();  
                            }                                                     
                        }
                    }
                    else
                    {
                        if (SoftwareBuffer.OutBuffTail != SoftwareBuffer.OutBuffHead)
                        {
                            TryReConnect();
                        }                            
                    }
                }
                catch (Exception ex)
                {
                    this.AllSent = 1;
                    MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    return (false);
                }
            }
            else if (this.SelectedComInterface == _pipe_t.pi_usb)
            {
                if (this._SerialPort == null)
                {
                    try
                    {
                        if (ComPort != "")
                        {
                            this.SetComPort(this.ComPort);
                        } 
                        else
                        {
                           // this.SelectedComInterface = _pipe_t.pi_modem_tcp;
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    if (this._SerialPort.IsOpen == true)
                    {
                        SoftwareBuffer.MaxMessageLength = 60;
                        try
                        {
                            if ((this.AllSent == 1) && (this.AllDone == 1))
                            {

                                byte[] buff = SoftwareBuffer.SuckFromOutBuff();
                                if (buff != null)
                                {
                                    StatusMessageFlag = stmsg._stmsg_sending;
                                    this.AllSent = 0;
                                    this._SerialPort.BaseStream.BeginWrite(buff, 0, buff.Length, ComSendCallback, (object)this._SerialPort);
                                   
                                }
                                else
                                {
                                    if (StatusMessageFlag == stmsg._stmsg_sending)
                                    {
                                        StatusMessageFlag = stmsg._stmsg_send_completed;
                                    }
                                    return (true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.AllSent = 1;
                           // MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);

                        }
                    }
                    else
                    {
                        if (SoftwareBuffer.OutBuffTail != SoftwareBuffer.OutBuffHead)
                        {
                            string[] s = SerialPort.GetPortNames();
                            if (s.ToList().Contains(_SerialPort.PortName))
                            {
                                try
                                {
                                    this._SerialPort.Open();
                                }
                                catch (Exception ex)
                                {
                                    SoftwareBuffer.OutBuffHead = 0;
                                    SoftwareBuffer.OutBuffTail = 0;
                                    this._SerialPort = null;
                                    MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }
            }
            return (false);
        }

        private void ComSendCallback(IAsyncResult ar)
        {
            SerialPort sp = (SerialPort)ar.AsyncState;
            try
            {
                if (sp.IsOpen != true)
                {
                    sp.Open();
                }
                if (sp.IsOpen == true)
                {
                    sp.BaseStream.EndWrite(ar);
                    this.AllSent = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.AllSent = 1;
        }
   
        public int ReceivePacket()
        {
            if (this.SelectedComInterface == _pipe_t.pi_modem_tcp)
            {
                try
                {
                    if(this.tcpClientSocket != null)
                    if ((this.tcpClientSocket.Available > 0) && (this.AllReceived == 1) && (this.AllDone == 1))
                    { 
                        StatusMessageFlag = stmsg._stmsg_received;
                        this.AllReceived = 0;
                        InLen = tcpClientSocket.Available;
                        this.tcpClientSocket.BeginReceive(this.InBuff, 0, InLen, SocketFlags.None, new AsyncCallback(_ReceiveCallback), this.tcpClientSocket);
                        return (1);
                    }
                }
                catch (Exception ex)
                {
                    this.tcpClientSocket = null;
                   // MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (this.SelectedComInterface == _pipe_t.pi_usb)
            {
                try
                {
                    if(this._SerialPort != null)
                    if (this._SerialPort.IsOpen)
                    {
                        if ((this._SerialPort.BytesToRead > 0) && (this._SerialPort.IsOpen == true) && (this.AllReceived == 1))
                        {                            
                            this.AllReceived = 0;
                            InLen = _SerialPort.BytesToRead;
                            this._SerialPort.BaseStream.BeginRead(this.InBuff, 0, InLen, ComReceiveCallback, this._SerialPort);
                            return (1);
                        }
                    }                   
                }
                catch (Exception ex)
                {
                   // MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            return (-1);
        }

        /* Name: _ReceiveCallback
        * Description: function will be called if data have been copied from socket to InBuff buffer; function will set
        *                 tcpAllReceived flag to 1, if data is received successfuly.
        * Argumets: ar - returned object from asynchronous process...
        * Return : None
        * */
        public void _ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //this.tcpClientSocket = (Socket)ar.AsyncState;
                StatusMessageFlag = stmsg._stmsg_received;
                this.tcpClientSocket.EndReceive(ar);
                
                if (SoftwareBuffer.AddToInBuff(InBuff, InLen) == false)
                {
                    // MessageBox.Show("Nera vietos buferyje", Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }                
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            AllReceived = 1;
        }
        /// <summary>
        /// Serial port com callback
        /// </summary>
        /// <param name="ar"></param>
        private void ComReceiveCallback(IAsyncResult ar)
        {
            SerialPort sp = (SerialPort)ar.AsyncState;
            try
            {
                if (sp.IsOpen)
                {
                    sp.BaseStream.EndRead(ar);
                }
                StatusMessageFlag = stmsg._stmsg_received;
                if (SoftwareBuffer.AddToInBuff(InBuff, InLen) == false)
                {
                   // MessageBox.Show("Nera vietos buferyje", Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.AllReceived = 1;
        }
        /* Name: _ConnectCallback
         * Description: function will be called if connection was established, or time out ocurred; function will set
         *                 tcpAllDone flag to 1 if connection is succesful or not susseccful;
         * Argumets: ar - returned object from asynchronous process...
         * Return : None
         * */
        public void _ConnectCallback(IAsyncResult ar)
        {
            try
            {
                this.tcpClientSocket = (Socket)ar.AsyncState;
                this.tcpClientSocket.EndConnect(ar);
                StatusMessageFlag = stmsg._stmsg_connected;              
            }
            catch (Exception ex)
            {   // after 3 asynchronous tries to connect exeption occurs: 
                StatusMessageFlag = stmsg._stmsg_connect_fail;
              //  this.tcpClientSocket.Dispose();
            }
            AllDone = 1;  // pranesam, kad baigtas jungimasis 
        }
        /* Name: TryDisconnect
        * Description: This function will try to disconnect socket to remote device. It sets tcpAllDone flag to 0. If exeption
        *                occured tcpAllDone flag will be set to 1
        * Argumets: None
        * Return : None
        * */
        public void TryDisconnect()
        {
            if (AllDone == 1)
            {
                if (this.SelectedComInterface == _pipe_t.pi_modem_tcp)
                {
                    try
                    {
                        AllDone = 0;
                        this.tcpClientSocket.BeginDisconnect(true, new AsyncCallback(this._DisconnectCallback), this.tcpClientSocket);
                    }
                    catch (Exception ex)
                    {   // after 3 asynchronous tries exeption occurs:                      
                        AllDone = 1;
                    }
                }
                else if (this.SelectedComInterface == _pipe_t.pi_usb)
                {
                    try
                    {
                        if (_SerialPort != null)
                        {
                            this._SerialPort.Close();
                        }
                        StatusMessageFlag = stmsg._stmsg_disconnected;
                    }
                    catch (Exception ex)
                    {
                     
                        AllDone = 1;
                    }
                }
                SetTimeOut(2);
            }
        }
        /* Name: _ConnectCallback
         * Description: function will be called if connection was established, or time out ocurred; function will set
         *                 tcpAllDone flag to 1 if connection is succesful or not susseccful;
         * Argumets: ar - returned object from asynchronous process...
         * Return : None
         * */
        public void _DisconnectCallback(IAsyncResult ar)
        {
            try
            {
                this.tcpClientSocket = (Socket)ar.AsyncState;
                this.tcpClientSocket.EndDisconnect(ar);
                StatusMessageFlag = stmsg._stmsg_disconnected;
            }
            catch (Exception ex)
            {   // after 3 asynchronous tries to connect exeption occurs:
                // MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            AllDone = 1;  // pranesam, kad baigtas atsijungimas 
        }
        /* Name: _SendCallback
         * Description: function will be called if data have been sent from socket to remote device socket; function will set
         *                 tcpAllSent flag to 1, if data is sent successfuly.
         * Argumets: ar - returned object from asynchronous process...
         * Return : None
         * */
        public void _SendCallback(IAsyncResult ar)
        {
            try
            {
                this.tcpClientSocket = (Socket)ar.AsyncState;
                this.tcpClientSocket.EndSend(ar);
                StatusMessageFlag = stmsg._stmsg_send_completed;
                AllSent = 1;                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.Current.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public string GetStatusMessage()
        {
            string s = "";

            if (this.StatusMessageFlag != this.StatusMessageOldFlag)
            {
                switch (StatusMessageFlag)
                {
                    case stmsg._stmsg_none:
                        s = "";
                        break;
                    case stmsg._stmsg_connecting:
                        s = "Connecting.";
                        break;
                    case stmsg._stmsg_connected:
                        s = "Connected.";
                        break;
                    case stmsg._stmsg_connected_start:
                        s = "Connected. Start.";
                        break;
                    case stmsg._stmsg_sending:
                        s = "Sending.";
                        break;
                    case stmsg._stmsg_send_timeout:
                        s = "Send time out.";
                        break;
                    case stmsg._stmsg_receive_timeout:
                        s = "Receive time out.";
                        break;
                    case stmsg._stmsg_disconnected:
                        s = "Disconnected.";
                        break;
                    case stmsg._stmsg_connecting_timeout:
                        s = "Connecting time out.";
                        break;
                    case stmsg._stmsg_read_device_version:
                        s = "Read device version.";
                        break;
                    case stmsg._stmsg_got_device_version:
                        s = "Got device version.";
                        break;
                    case stmsg._stmsg_finishedupdatepleasewait:
                        s = "Finished updating. Please wait.";
                        break;  
                    case stmsg._stmsg_finished:
                        s = "Finished.";
                        break;                   
                    case stmsg._stmsg_waitting:
                        s = "Waitting.";
                        break;
                    case stmsg._stmsg_stop:
                        s = " ";// stopped
                        break;                 
                    case stmsg._stmsg_connect_fail:
                        s = "Connect fail";// com fail
                        break;
                    case stmsg._stmsg_portsetupfail:
                        s = "Port setup fail";// com fail
                        break;
                    default:
                        s = "";
                        break;
                }
                this.StatusMessageOldFlag = this.StatusMessageFlag;
            }

            return (s);
        }

        public stmsg GetStatus()
        {
            stmsg rValue = stmsg._stmsg_none;
            if (this.StatusMessageFlag != stmsg._stmsg_none)
            {
                rValue = StatusMessageFlag;
                StatusMessageFlag = stmsg._stmsg_none;
            }

            return (rValue);
        }

    }

    public class SoftwareBuff : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public byte[] OutBuff;
        public byte[] InBuff;

        private int _OutBuffHead;
        public int OutBuffHead
        {
            get { return _OutBuffHead; }
            set
            {
                _OutBuffHead = value;
                NotifyPropertyChanged("OutBuffHead");
            }
        }

        private int _OutBuffTail;
        public int OutBuffTail
        {
            get { return _OutBuffTail; }
            set
            {
                _OutBuffTail = value;
                NotifyPropertyChanged("OutBuffTail");
            }
        }

        private int _InBuffHead;
        public int InBuffHead
        {
            get { return _InBuffHead; }
            set
            {
                _InBuffHead = value;
                NotifyPropertyChanged("InBuffHead");
            }
        }

        private int _InBuffTail;
        public int InBuffTail
        {
            get { return _InBuffTail; }
            set
            {
                _InBuffTail = value;
                NotifyPropertyChanged("InBuffTail");
            }
        }

        public int MaxMessageLength;

        public SoftwareBuff()
        {
            this.OutBuff = new byte[4096];
            this.InBuff = new byte[4096];
            this.OutBuffHead = new int();
            this.OutBuffTail = new int();
            this.InBuffHead = new int();
            this.InBuffTail = new int();
            this.MaxMessageLength = new int();
            this.MaxMessageLength = 64;
        }

        public byte[] SuckFromBuff(byte[] buffer, int head, int tail)
        {
            if (tail != head)
            {
                int bytecount = new int();
                if (head > tail)
                {
                    bytecount = Math.Abs(head - tail);
                }
                else
                {
                    bytecount = buffer.Length + head - tail;
                }
                if (bytecount > this.MaxMessageLength)
                {
                    bytecount = this.MaxMessageLength;
                }
                byte[] bytearray = new byte[bytecount];

                int i = new int();
                i = 0;

                while (tail != head)
                {
                    bytearray[i++] = buffer[tail];
                    if (tail >= (buffer.Length - 1))
                    {
                        tail = 0;
                    }
                    else
                    {
                        tail++;
                    }
                    if (i == bytecount)
                    {
                        return (bytearray);
                    }
                }
                return (bytearray);
            }
            return (null);
        }
        public bool AddToBuff(byte[] ByteArray, int ByteCount, byte[] DestinationBuff, int DestinationBuffHead, int DestinationBuffTail)
        {
            if (IsAnyPlaceInTheBuffer(DestinationBuff, ByteCount, DestinationBuffHead, DestinationBuffTail))
            {
                for (int i = 0; i < ByteCount; i++)
                {
                    DestinationBuff[DestinationBuffHead] = ByteArray[i];
                    if (DestinationBuffHead >= (DestinationBuff.Length - 1))
                    {
                        DestinationBuffHead = 0;
                    }
                    else
                    {
                        DestinationBuffHead++;
                    }
                }
            }
            else
            {
                return (false);
            }
            return (true);
        }
        private bool IsAnyPlaceInTheBuffer(byte[] buffer, int byteCount, int head, int tail)
        {
            int temp_head = new int();
            temp_head = head;

            for (int i = 0; i < byteCount; i++)
            {
                if (temp_head >= (buffer.Length - 1))
                {
                    if (0 != tail)
                    {
                        temp_head = 0;
                    }
                    else
                    {
                        return (false);
                    }
                }
                else
                {
                    if ((temp_head + 1) != tail)
                    {
                        temp_head++;
                    }
                    else
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }
        public byte[] SuckFromOutBuff()
        {
            if (OutBuffTail != OutBuffHead)
            {
                int bytecount = new int();
                if (OutBuffHead > OutBuffTail)
                {
                    bytecount = Math.Abs(OutBuffHead - OutBuffTail);
                }
                else
                {
                    bytecount = OutBuff.Length + OutBuffHead - OutBuffTail;
                }
                if (bytecount > this.MaxMessageLength)
                {
                    bytecount = this.MaxMessageLength;
                }
                byte[] bytearray = new byte[bytecount];

                int i = new int();
                i = 0;

                while (OutBuffTail != OutBuffHead)
                {
                    bytearray[i++] = OutBuff[OutBuffTail];
                    if (OutBuffTail >= (OutBuff.Length - 1))
                    {
                        OutBuffTail = 0;
                    }
                    else
                    {
                        OutBuffTail++;
                    }
                    if (i == bytecount)
                    {
                        return (bytearray);
                    }
                }
                return (bytearray);
            }
            return (null);
        }
        /// <summary>
        /// Funkcija kopijuoja baitus iš ByteArray į globalų buferį OutBuff
        /// </summary>
        /// <param name="ByteArray">nauji duomenys</param>
        /// <param name="ByteCount">duomenu kiekis</param>
        /// <returns>Jei nėra vietos naujiems duomenims grazina false </returns>
        public bool AddToOutBuff(byte[] ByteArray, int ByteCount)
        {
            if (IsAnyPlaceInTheBuffer(OutBuff, ByteCount, OutBuffHead, OutBuffTail))
            {
                for (int i = 0; i < ByteCount; i++)
                {
                    OutBuff[OutBuffHead] = ByteArray[i];
                    if (OutBuffHead >= (OutBuff.Length - 1))
                    {
                        OutBuffHead = 0;
                    }
                    else
                    {
                        OutBuffHead++;
                    }
                }
            }
            else
            {
                return (false);
            }
            return (true);
        }
        /// <summary>
        /// Funkcija kopijuoja baitus iš ByteArray į globalų buferį InBuff
        /// </summary>
        /// <param name="ByteArray">nauji duomenys</param>
        /// <param name="ByteCount">duomenu kiekis</param>
        /// <returns>Jei nėra vietos naujiems duomenims grazina false </returns>
        public bool AddToInBuff(byte[] ByteArray, int ByteCount)
        {
            if (IsAnyPlaceInTheBuffer(InBuff, ByteCount, InBuffHead, InBuffTail))
            {
                for (int i = 0; i < ByteCount; i++)
                {
                    InBuff[InBuffHead] = ByteArray[i];
                    if (InBuffHead >= (InBuff.Length - 1))
                    {
                        InBuffHead = 0;
                    }
                    else
                    {
                        InBuffHead++;
                    }
                }
            }
            else
            {
                return (false);
            }
            return (true);
        }

        public byte[] GetNextByteFromInBuff()
        {
            byte[] b = new byte[1];
            if (InBuffTail != InBuffHead)
            {
                b[0] = InBuff[InBuffTail];
                if (InBuffTail >= (InBuff.Length - 1))
                {
                    InBuffTail = 0;
                }
                else
                {
                    InBuffTail++;
                }
                return (b);
            }
            return (null);
        }


        public byte[] SuckFromInBuff()
        {
            if (InBuffTail != InBuffHead)
            {
                int bytecount = new int();
                if (InBuffHead > InBuffTail)
                {
                    bytecount = Math.Abs(InBuffHead - InBuffTail);
                }
                else
                {
                    bytecount = InBuff.Length + InBuffHead - InBuffTail;
                }
                if (bytecount > 1024/*this.MaxMessageLength*/)
                {
                    bytecount = 1024/*this.MaxMessageLength*/;
                }
                byte[] bytearray = new byte[bytecount];

                int i = new int();
                i = 0;

                while (InBuffTail != InBuffHead)
                {
                    bytearray[i++] = InBuff[InBuffTail];
                    if (InBuffTail >= (InBuff.Length - 1))
                    {
                        InBuffTail = 0;
                    }
                    else
                    {
                        InBuffTail++;
                    }
                    if (i == bytecount)
                    {
                        return (bytearray);
                    }
                }
                return (bytearray);
            }
            return (null);
        }

    }
}
