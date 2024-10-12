using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace usbcom
{
    public class tLogItem : INotifyPropertyChanged
    {
        private string _LogTimeString;
        public string LogTimeString
        {
            get { return _LogTimeString; }
            set
            {
                _LogTimeString = value;
                NotifyPropertyChanged("LogTimeString");
            }
        }
        private string _LogString;
        public string LogString
        {
            get { return _LogString; }
            set
            {
                _LogString = value;
                NotifyPropertyChanged("LogString");
            }
        }

        private string _LogDirection;
        public string LogDirection
        {
            get { return _LogDirection; }
            set
            {
                _LogDirection = value;
                NotifyPropertyChanged("LogDirection");
            }
        }

        private Brush _LogColor;
        public Brush LogColor
        {
            get { return _LogColor; }
            set
            {
                _LogColor = value;
                NotifyPropertyChanged("LogColor");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    };

}
