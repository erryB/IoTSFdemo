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
        protected override bool AreArgumentsOk(SetDeviceConfigurationParameters arguments)
        {
            var result = base.AreArgumentsOk(arguments);
            if (arguments.CreateFile && string.IsNullOrWhiteSpace(arguments.FileName))
            {
                DisplayError("E' necessario impostare il nome del file per poter generare il file JSON.");
                result = false;
            }
            if (!arguments.CreateFile && string.IsNullOrWhiteSpace(arguments.ServiceUri))
            {
                DisplayError("Il parametro serviceUri e' obbligatorio");
                result = false;
            }
            if (!arguments.CreateFile && string.IsNullOrWhiteSpace(arguments.DeviceId))
            {
                DisplayError("Il parametro deviceId e' obbligatorio");
                result = false;
            }
            if (arguments.OpenDoorDuration.HasValue && arguments.OpenDoorDuration.Value <= 0)
            {
                DisplayError("Il parametro doorDuration deve essere maggiore di 0");
                result = false;
            }
            return result;
        }

        protected override void ShowCommandDescription()
        {
            this.DisplayMessage(
                "Esegue il comando di SetDeviceConfigurationAsync di un device.");
        }


        protected override bool Execute(SetDeviceConfigurationParameters arguments)
        {
            return ExecuteAsync(arguments).GetAwaiter().GetResult();
        }


        private async Task<bool> ExecuteAsync(SetDeviceConfigurationParameters arguments)
        {
            try
            {
                DeviceConfigurationData config = null;
                if (!arguments.CreateFile && !string.IsNullOrWhiteSpace(arguments.FileName))
                {
                    string jsonConfig = await FileHelper.ReadAllTextAsync(arguments.FileName);
                    config = JsonConvert.DeserializeObject<DeviceConfigurationData>(jsonConfig);
                }
                else
                {
                    config = new DeviceConfigurationData();
                    if (arguments.TemperatureThreshold.HasValue)
                        config.Properties[DeviceConfigurationPropertyNames.TemperatureThresholdName] = arguments.TemperatureThreshold.Value;
                    if (arguments.HumidityThreshold.HasValue)
                        config.Properties[DeviceConfigurationPropertyNames.HumidityThresholdName] = arguments.HumidityThreshold.Value;
                    if (arguments.OpenDoorDuration.HasValue)
                        config.Properties[DeviceConfigurationPropertyNames.OpenDoorDutationName] = TimeSpan.FromSeconds(arguments.OpenDoorDuration.Value);
                }

                if (arguments.CreateFile)
                {
                    var jsonConfig = JsonConvert.SerializeObject(config, Formatting.Indented);
                    this.DisplayMessage($"Inizio scrittura file {arguments.FileName }");
                    await FileHelper.WriteAllTextAsync(arguments.FileName, jsonConfig);
                    this.DisplayMessage($"Fine scrittura su attore file {arguments.FileName }");
                }
                else
                {
                    var actorId = new ActorId(arguments.DeviceId);
                    var serviceUri = new Uri(arguments.ServiceUri);
                    var actorProxy = ActorProxy.Create<IDeviceConfiguration>(actorId, serviceUri);
                    this.DisplayMessage($"Inizio SetDeviceConfigurationAsync su attore {serviceUri} con id {actorId}");
                    var sw = Stopwatch.StartNew();
                    var result = await actorProxy.SetConfigurationAsync(config, default(CancellationToken));
                    sw.Stop();
                    this.DisplayMessage($"Fine SetDeviceConfigurationAsync su attore {serviceUri} con id {actorId } - Durata {sw.ElapsedMilliseconds } msec - Risultato {result}");
                }

            }
            catch (Exception ex)
            {
                DisplayError(ex.ToString());
            }
            return true;
        }

    }
}
