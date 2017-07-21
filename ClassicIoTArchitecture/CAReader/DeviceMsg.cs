using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CAReader
{
    [DataContract]
    public class DeviceMsg 
    {
        [DataMember]
        public string DeviceID { get; set; }
        [DataMember]
        public int MsgID { get; set; }
        [DataMember]
        public DateTime TimeStamp { get; set; }
        [DataMember]
        public string Data { get; set; }

    }   
}
