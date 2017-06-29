using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAReader
{
    class DeviceMsg
    {
        public string DeviceID { get; set; }
        public int MsgID { get; set; }
        public DateTime TimeStamp { get; set; }
        public object Data { get; set; }
        //public double Temperature { get; set; }
        //public double Humidity { get; set; }
        //public bool DoorOpen { get; set; }


    }   
}
