using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usbcom
{

    public class LogItems : INotifyPropertyChanged,INotifyCollectionChanged
    {
        private bool _Hex;
        public bool Hex
        {
            get { return _Hex; }
            set
            {
                _Hex = value;
                NotifyPropertyChanged("Hex");
            }
        }

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

        private ObservableCollection<tLogItem> _Items;
        public ObservableCollection<tLogItem> Items
        {
            get { return _Items; }
            set
            {
                _Items = value;
                NotifyPropertyChanged("Items");
            }
        }


        public LogItems()
        {
            this.PropertyChanged += LogItems_PropertyChanged;
            Items = new ObservableCollection<tLogItem>();
            Items.CollectionChanged += CollectionChanged;
            ;
            Enable = true;
            Hex = true;
        }
      
        private void LogItems_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Hex")
            {
                Enable = false;
                string[] ss = new string[Items.Count];

                for (int i = 0; i < ss.Length; i++)
                {
                    if (Hex)
                    {
                        ss[i] = GetLogLine(Encoding.UTF8.GetBytes(Items[i].LogString), Hex);
                    }
                    else
                    {
                        string[] s = Items[i].LogString.Split(' ');
                        string s2 = "";
                        for (int j = 0; j < s.Length; j++)
                        {
                            s2 += s[j];
                        }
                        ss[i] = GetLogLine(StringToByteArray(s2), Hex);
                    }
                    Items[i].LogString = ss[i];
                }
                Enable = true;
            }
        }

        public static string GetLogLine(byte[] data, bool hex)
        {
            if (data == null) return ("");
            string s = "";
            for (int i = 0; i < data.Length; i++)
            {
                if (hex)
                {
                    s += data[i].ToString("X2") + " ";
                }
                else
                {
                    byte[] xs = { data[i], 0 };
                    s += Encoding.UTF8.GetString(xs, 0, 1);
                }
            }
            return (s);
        }

        public static byte[] StringToByteArray(string hex)
        {
            try
            {
                return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
            }
            catch
            {
                return (null);
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

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void NotifyCollectionChanged (object sender ,NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke (this ,new NotifyCollectionChangedEventArgs (e.Action));
        }

    }


}
