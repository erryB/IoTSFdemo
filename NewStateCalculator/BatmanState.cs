using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewStateCalculator
{
    [Serializable]
    class BatmanState
    {
        public string DeviceID { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public int CountTempIncreasing { get; set; }
        public int CountHumIncreasing { get; set; }
    }
}
