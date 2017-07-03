using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CAReader
{
    [Serializable]
    public class DeviceMsg 
    {
        public string DeviceID { get; set; }
        public int MsgID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Data { get; set; }

    }   
}
