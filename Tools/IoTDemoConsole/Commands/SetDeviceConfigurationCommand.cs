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

    public class SetDeviceConfigurationCommand : ConsoleCommandBase<SetDeviceConfigurationParameters>
    {

        protected override void ShowCommandDescription()
        {
            this.DisplayMessage(
                "Esegue il comando di SetDeviceConfigurationAsync di un device recuperando la configurazione da un file JSON.");
        }


        protected override bool Execute(SetDeviceConfigurationParameters arguments)
        {
            return ExecuteAsync(arguments).GetAwaiter().GetResult();
        }


        private async Task<bool> ExecuteAsync(SetDeviceConfigurationParameters arguments)
        {
            try
            {
                var actorId = new ActorId(arguments.DeviceId);
                var serviceUri = new Uri(arguments.ServiceUri);
                var actorProxy = ActorProxy.Create<IDeviceConfiguration>(actorId, serviceUri);
                string jsonConfig = await FileHelper.ReadAllTextAsync(arguments.FileName);
                DeviceConfiguration config = JsonConvert.DeserializeObject<DeviceConfiguration>(jsonConfig);
                this.DisplayMessage($"Inizio SetDeviceConfigurationAsync su attore {serviceUri} con id {actorId}");
                var sw = Stopwatch.StartNew();
                var result = await actorProxy.SetConfigurationAsync(config, default(CancellationToken));
                sw.Stop();
                this.DisplayMessage($"Fine SetDeviceConfigurationAsync su attore {serviceUri} con id {actorId } - Durata {sw.ElapsedMilliseconds } msec - Risultato {result}");
            }
            catch (Exception ex)
            {
                DisplayError(ex.ToString());
            }
            return true;
        }

    }
}
