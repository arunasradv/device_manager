using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usbcom
{
    public class DeviceList
    {
        public ObservableCollection<DeviceListItem> Items { get; set; }

        public DeviceList()
        {
            Items = new ObservableCollection<DeviceListItem>();
        }
    }
}
