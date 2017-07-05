using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewStateCalculator
{
    [Serializable]
    class JokerState
    {
        public double Temperature { get; set; }
        public bool IsOpen { get; set; }
        public int TempIncreasingCount { get; set; }
        public int DoorOpenCount { get; set; }
    }
}
