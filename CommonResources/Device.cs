using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonResources
{
    [Serializable]
    public class Device
    {
        public string DeviceID { get; set; }
        public int MessageID { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public bool OpenDoor { get; set; }
        public DateTime Timestamp { get; set; }
      
    }
}
