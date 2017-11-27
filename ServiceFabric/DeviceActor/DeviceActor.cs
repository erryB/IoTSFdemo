using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceActor.Interfaces;
using CommonResources;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using AlarmWriterService.Interfaces;
using Newtonsoft.Json;
using System.Text;
using Microsoft.ServiceFabric.Services.Client;

namespace DeviceActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class- n.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    [ActorService(Name = "DeviceActor")]
    internal class DeviceActor : Actor, IDeviceConfiguration, IDeviceActor, IRemindable
    {

        /// <summary>
        /// Initializes a new instance of DeviceActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public DeviceActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        private const string PreviousTemperatureStateKey = "PreviousTemperatureState";
        private const string PreviousHumidityStateKey = "PreviousHumidityState";
        private const string LastOpenDoorTimeStateKey = "LastOpenDoorTimeState";
        private const string LastDeviceMassageStateKey = "LastDeviceMessageState";

        private const string SendAlarmMessageReminderName = "SendAlarmMessageReminder";
        private const string SendAlarmMessageQueueName = "SendAlarmMessageQueue";
        private const int SendAlarmMessageReminderDueTimeInMSec = 100;

        public async Task UpdateDeviceStateAsync(DeviceMessage currentDeviceMessage, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, $"Update message arrived. {currentDeviceMessage.MessageType }");
            Object alarmMsg = null;

            var lastDeviceMessage = await this.StateManager.GetOrAddStateAsync<DeviceMessage>(LastDeviceMassageStateKey, null, cancellationToken);
            if (lastDeviceMessage == null || lastDeviceMessage.Timestamp < currentDeviceMessage.Timestamp) // drop automatically the oldest messages from the last arrived
            {
                await this.StateManager.SetStateAsync<DeviceMessage>(LastDeviceMassageStateKey, currentDeviceMessage, cancellationToken);
                if (currentDeviceMessage.MessageType == MessagePropertyName.TempHumType)
                {
                    alarmMsg = await CheckMessageForTemperatureHumidityDevice(currentDeviceMessage, cancellationToken);
                }
                else if (currentDeviceMessage.MessageType == MessagePropertyName.TempOpenDoorType)
                {
                    alarmMsg = await CheckMessageForTemperatureOpenDoorDevice(currentDeviceMessage, cancellationToken);
                }
                else if (currentDeviceMessage.MessageType == MessagePropertyName.UnknownType)
                {
                    alarmMsg = new
                    {
                        DeviceID = "unknown",
                        MessageID = "unknown",
                        AlarmMessage = "ALARM! Unknown device",
                        Timestamp = DateTime.Now
                    };
                }

                if (alarmMsg != null)
                {
                    var messageString = JsonConvert.SerializeObject(alarmMsg);
                    await this.StateManager.EnqueueAsync(SendAlarmMessageQueueName, messageString, cancellationToken);
                    await RegisterReminderAsync(SendAlarmMessageReminderName, null, TimeSpan.FromMilliseconds(SendAlarmMessageReminderDueTimeInMSec), TimeSpan.FromMilliseconds(-1));

                }
            }
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var reminder = GetReminder(reminderName);
            await UnregisterReminderAsync(reminder);

            if (reminderName == SendAlarmMessageReminderName)
            {
                ActorEventSource.Current.ActorMessage(this, $"Reminder {SendAlarmMessageReminderName} received.");
                var messageString = await this.StateManager.PeekQueueAsync<string>(SendAlarmMessageQueueName);
                if (messageString != null)
                {
                    try
                    {
                        //ActorEventSource.Current.ActorMessage(this, "Sending to AlarmServiceWriter - {0}.", messageString);
                        await AlarmServiceWriterProxy.SendAlarmAsync(this.Id.ToString(), messageString);
                        ActorEventSource.Current.ActorMessage(this, "Sent to AlarmServiceWriter - {0}.", messageString);
                        await this.StateManager.DequeueAsync<string>(SendAlarmMessageQueueName);
                    }
                    catch (Exception ex)
                    {
                        ActorEventSource.Current.ActorMessage(this, $"[EXCEPTION] {0}", ex);
                    }
                }
                if (await this.StateManager.GetQueueLengthAsync(SendAlarmMessageQueueName) > 0)
                    await RegisterReminderAsync(SendAlarmMessageReminderName, null, TimeSpan.FromMilliseconds(SendAlarmMessageReminderDueTimeInMSec), TimeSpan.FromMilliseconds(-1));

            }
        }

        private async Task<object> CheckMessageForTemperatureOpenDoorDevice(DeviceMessage currentDeviceMessage, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, $"Check Message TemperatureOpenDoorDevice.");
            object alarmMsg = null;
            var startOpenDoorTime = await this.StateManager.TryGetStateAsync<DateTime>(LastOpenDoorTimeStateKey, cancellationToken);
            var currentTemperature = Double.Parse(currentDeviceMessage.MessageData[MessagePropertyName.Temperature]);
            var temperatureThreshold = await this.StateManager.TryGetStateAsync<double>(DeviceConfigurationPropertyNames.TemperatureThresholdName, cancellationToken);
            var openDoorDurationThreshold = await this.StateManager.TryGetStateAsync<TimeSpan>(DeviceConfigurationPropertyNames.OpenDoorDutationName, cancellationToken);

            if (openDoorDurationThreshold.HasValue && temperatureThreshold.HasValue)
            {
                var doorIsOpen = currentDeviceMessage.MessageData[MessagePropertyName.OpenDoor] == "True";
                if (doorIsOpen)
                {
                    if (startOpenDoorTime.HasValue)
                    {
                        if (currentDeviceMessage.Timestamp.Subtract(startOpenDoorTime.Value) > openDoorDurationThreshold.Value &&
                            currentTemperature > temperatureThreshold.Value)
                        {
                            alarmMsg = new
                            {
                                DeviceID = currentDeviceMessage.DeviceID,
                                MessageID = currentDeviceMessage.MessageID,
                                AlarmMessage =
                                $"The door is still open and the temperature is {currentDeviceMessage.MessageData[MessagePropertyName.Temperature]}. PLEASE CLOSE THE DOOR!",
                                Timestamp = DateTime.Now
                            };
                        }
                    }
                    else
                    {
                        await this.StateManager.SetStateAsync<DateTime>(LastOpenDoorTimeStateKey, currentDeviceMessage.Timestamp, cancellationToken);
                    }

                }

                await this.StateManager.AddOrUpdateStateAsync<double>(PreviousTemperatureStateKey, currentTemperature,
                    (x, y) => currentTemperature, cancellationToken);
            }
            return alarmMsg;
        }

        private async Task<object> CheckMessageForTemperatureHumidityDevice(DeviceMessage currentDeviceMessage, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, $"Check Message TemperatureHumidityDevice.");
            object alarmMsg = null;
            var previousTemperature = await this.StateManager.TryGetStateAsync<double>(PreviousTemperatureStateKey, cancellationToken);
            var currentTemperature = Double.Parse(currentDeviceMessage.MessageData[MessagePropertyName.Temperature]);
            var currentHumidity = Double.Parse(currentDeviceMessage.MessageData[MessagePropertyName.Humidity]);
            var temperatureThreshold = await this.StateManager.TryGetStateAsync<double>(DeviceConfigurationPropertyNames.TemperatureThresholdName, cancellationToken);
            var humidityTemperatureThreshold = await this.StateManager.TryGetStateAsync<double>(DeviceConfigurationPropertyNames.HumidityThresholdName, cancellationToken);

            if (temperatureThreshold.HasValue && humidityTemperatureThreshold.HasValue)
            {
                if (previousTemperature.HasValue &&
                    currentTemperature > temperatureThreshold.Value &&
                    //currentTemperature > previousTemperature.Value &&
                    currentHumidity > humidityTemperatureThreshold.Value)
                {
                    alarmMsg = new
                    {
                        DeviceID = currentDeviceMessage.DeviceID,
                        MessageID = currentDeviceMessage.MessageID,
                        AlarmMessage =
                        $"Too high temperature and humidity! Temperature: {currentTemperature}, Humidity: {currentHumidity}",
                        Timestamp = DateTime.Now
                    };

                    //alarmMsg = $"ALARM generated by {currentDeviceMessage.DeviceID}, MessageID {currentDeviceMessage.MessageID}: too high temperature and humidity! Temperature: {currentDeviceMessage.MessageData[MessagePropertyName.Temperature]}, Humidity: {currentDeviceMessage.MessageData[MessagePropertyName.Humidity]}";
                }
            }
            else
            {
                ActorEventSource.Current.ActorMessage(this, "Device not configured!!!");
            }

            await this.StateManager.AddOrUpdateStateAsync<double>(PreviousTemperatureStateKey, currentTemperature,
                (x, y) => currentTemperature, cancellationToken);
            await this.StateManager.AddOrUpdateStateAsync<double>(PreviousHumidityStateKey, currentHumidity,
                (x, y) => currentHumidity, cancellationToken);

            ActorEventSource.Current.ActorMessage(this, "DeviceActor - State has been updated");

            return alarmMsg;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return Task.Delay(0);
        }

        #region [ IDeviceConfiguration methods ]
        /// <summary>
        /// Gets the configuration asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;DeviceConfigurationData&gt;.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<DeviceConfigurationData> GetConfigurationAsync(CancellationToken cancellationToken)
        {
            DeviceConfigurationData config = new DeviceConfigurationData();

            await GetConfigurationAsync<double>(DeviceConfigurationPropertyNames.TemperatureThresholdName, config, cancellationToken);
            await GetConfigurationAsync<double>(DeviceConfigurationPropertyNames.HumidityThresholdName, config, cancellationToken);
            await GetConfigurationAsync<TimeSpan>(DeviceConfigurationPropertyNames.OpenDoorDutationName, config, cancellationToken);

            return config;
        }

        /// <summary>
        /// Sets the configuration asynchronous.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> SetConfigurationAsync(DeviceConfigurationData config, CancellationToken cancellationToken)
        {
            if (config == null)
                throw new NullReferenceException(nameof(config));

            bool result = true;

            result &= await SetConfigurationValueAsync<double>(DeviceConfigurationPropertyNames.TemperatureThresholdName, config, cancellationToken);
            result &= await SetConfigurationValueAsync<double>(DeviceConfigurationPropertyNames.HumidityThresholdName, config, cancellationToken);
            result &= await SetConfigurationValueAsync<TimeSpan>(DeviceConfigurationPropertyNames.OpenDoorDutationName, config, cancellationToken);

            return result;
        }
        #endregion [ IDeviceConfiguration methods ]

        #region [ Private methods ]

        private async Task<bool> SetConfigurationValueAsync<T>(string configurationName, DeviceConfigurationData config, CancellationToken cancellationToken)
        {
            bool result = true;

            if (config.Properties.ContainsKey(configurationName))
            {
                object objvalue = config.Properties[configurationName];
                if (objvalue is T variable)
                {
                    await this.StateManager.SetStateAsync<T>(configurationName, variable, cancellationToken);
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        private async Task GetConfigurationAsync<T>(string configurationName, DeviceConfigurationData config, CancellationToken cancellationToken)
        {
            var value = await this.StateManager.TryGetStateAsync<T>(configurationName, cancellationToken);
            if (value.HasValue)
                config.Properties[configurationName] = value.Value;
        }

        #endregion [ Private methods ]
    }
}
