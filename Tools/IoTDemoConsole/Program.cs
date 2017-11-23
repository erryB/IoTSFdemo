using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonResources;
using IoTDemoConsole.Commands;
using IoTDemoConsole.Outputs;
using Newtonsoft.Json;

namespace IoTDemoConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new CommandSelector(new StandardConsoleOutput())
                .SetConsoleDescription("Console di test per la demo")
                .RegisterCommand<SetDeviceConfigurationCommand>("setconfig")
                .RegisterCommand<GetDeviceConfigurationCommand>("getconfig")
                .RegisterCommand<SendDeviceMessageCommand>("sendcommand")
                .ThenSelectAndExecute(args);

        }
    }
}
