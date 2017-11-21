using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTDemoConsole.Commands;
using IoTDemoConsole.Outputs;

namespace IoTDemoConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new CommandSelector(new StandardConsoleOutput())
                .RegisterCommand<SetDeviceConfigurationCommand>("setconfig")
                .ThenSelectAndExecute(args);
        }
    }
}
