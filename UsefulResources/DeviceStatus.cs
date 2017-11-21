using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UsefulResources
{
    [DataContract]
    public class DeviceStatus
    {
        [DataMember]
        public string DeviceID { get; set; }

        //*****necessary???*****
        [DataMember]
        public int MessageID { get; set; }

        [DataMember]
        public string DeviceType { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public Dictionary<string, string> Data { get; set; }
    }
}
