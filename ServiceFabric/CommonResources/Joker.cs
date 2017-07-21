using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonResources
{
    [DataContract]
    public class Joker : Device
    {
        [DataMember]
        public double Temperature { get; set; }
        [DataMember]
        public bool OpenDoor { get; set; }
        
    }
}
