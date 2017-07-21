using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonResources
{
    public class JokerState
    {
        public double CurrentTemperature { get; set; }
        public bool CurrentlyOpen { get; set; }
        public int IncreasingTemperatureCount { get; set; }
        public int OpenDoorCount { get; set; }

    }
}
