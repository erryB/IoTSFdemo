using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonResources
{
    [DataContract]
    public class Device
    {
        [DataMember]
        public string DeviceID { get; set; }
        [DataMember]
        public int MessageID { get; set; }
        [DataMember]
        public DateTime Timestamp { get; set; }
      
    }
}
