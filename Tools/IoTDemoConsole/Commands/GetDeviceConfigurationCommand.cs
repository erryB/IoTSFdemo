using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommonResources;
using DeviceActor.Interfaces;
using IoTDemoConsole.CommandParameters;
using IoTDemoConsole.Helpers;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Newtonsoft.Json;

namespace IoTDemoConsole.Commands
{

    public class GetDeviceConfigurationCommand : ConsoleCommandBase<GetDeviceConfigurationParameters>
    {
        protected override void ShowCommandDescription()
        {
            this.DisplayMessage(
                "Esegue il comando di GetDeviceConfigurationAsync di un device.");
        }


        protected override bool Execute(GetDeviceConfigurationParameters arguments)
        {
            return ExecuteAsync(arguments).GetAwaiter().GetResult();
        }


        private async Task<bool> ExecuteAsync(GetDeviceConfigurationParameters arguments)
        {
            try
            {
                var actorId = new ActorId(arguments.DeviceId);
                var serviceUri = new Uri(arguments.ServiceUri);
                var actorProxy = ActorProxy.Create<IDeviceConfiguration>(actorId, serviceUri);
                this.DisplayMessage($"Inizio GetDeviceConfigurationAsync su attore {serviceUri} con id {actorId}");
                var sw = Stopwatch.StartNew();
                var result = await actorProxy.GetConfigurationAsync( default(CancellationToken));
                sw.Stop();
                this.DisplayMessage($"Fine GetDeviceConfigurationAsync su attore {serviceUri} con id {actorId } - Durata {sw.ElapsedMilliseconds } msec");
                var resultStr = JsonConvert.SerializeObject(result, Formatting.Indented);
                this.DisplayMessage($"{resultStr}");
            }
            catch (Exception ex)
            {
                DisplayError(ex.ToString());
            }
            return true;
        }

    }
}
