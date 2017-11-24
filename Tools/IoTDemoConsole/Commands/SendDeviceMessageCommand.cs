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

    public class SendDeviceMessageCommand : ConsoleCommandBase<SendDeviceMessageParameters>
    {
        protected override bool AreArgumentsOk(SendDeviceMessageParameters arguments)
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
            return result;
        }

        protected override void ShowCommandDescription()
        {
            this.DisplayMessage(
                "Esegue il comando di SetDeviceConfigurationAsync di un device.");
        }


        protected override bool Execute(SendDeviceMessageParameters arguments)
        {
            return ExecuteAsync(arguments).GetAwaiter().GetResult();
        }


        private async Task<bool> ExecuteAsync(SendDeviceMessageParameters arguments)
        {
            try
            {
                DeviceMessage message = null;
                if (!arguments.CreateFile && !string.IsNullOrWhiteSpace(arguments.FileName))
                {
                    string jsonConfig = await FileHelper.ReadAllTextAsync(arguments.FileName);
                    message = JsonConvert.DeserializeObject<DeviceMessage>(jsonConfig);
                }
                else
                {
                    message = new DeviceMessage();
                    message.DeviceID = arguments.DeviceId;
                    if (arguments.Temperature.HasValue && arguments.Humidity.HasValue)
                    {
                        message.MessageType = MessagePropertyName.TempHumType;
                        message.MessageData.Add(MessagePropertyName.Temperature, arguments.Temperature.Value.ToString());
                        message.MessageData.Add(MessagePropertyName.Humidity, arguments.Humidity.Value.ToString());

                    }
                    else if (arguments.Temperature.HasValue && arguments.DoorOpen.HasValue)
                    {
                        message.MessageType = MessagePropertyName.TempOpenDoorType;
                        message.MessageData.Add(MessagePropertyName.Temperature, arguments.Temperature.Value.ToString());
                        message.MessageData.Add(MessagePropertyName.OpenDoor, arguments.DoorOpen.Value.ToString());
                    }
                    else
                    {
                        message.MessageType = MessagePropertyName.UnknownType;
                    }
                }

                if (arguments.CreateFile)
                {
                    var jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented);
                    this.DisplayMessage($"Inizio scrittura file {arguments.FileName }");
                    await FileHelper.WriteAllTextAsync(arguments.FileName, jsonMessage);
                    this.DisplayMessage($"Fine scrittura su attore file {arguments.FileName }");
                }
                else
                {
                    var actorId = new ActorId(arguments.DeviceId);
                    var serviceUri = new Uri(arguments.ServiceUri);
                    var actorProxy = ActorProxy.Create<IDeviceActor>(actorId, serviceUri);
                    this.DisplayMessage($"Inizio SendDeviceMessageAsync su attore {serviceUri} con id {actorId}");
                    var sw = Stopwatch.StartNew();
                    await actorProxy.UpdateDeviceStateAsync(message, default(CancellationToken));
                    sw.Stop();
                    this.DisplayMessage($"Fine SendDeviceMessageAsync su attore {serviceUri} con id {actorId } - Durata {sw.ElapsedMilliseconds } msec");
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
