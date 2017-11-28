﻿using System;
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
    [ActorService(Name = "TDDeviceActor")]
    public class TDDeviceActor : DeviceActorBase, IDeviceConfiguration, IDeviceActor
    {

        /// <summary>
        /// Initializes a new instance of DeviceActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public TDDeviceActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected const string PreviousTemperatureStateKey = "PreviousTemperatureState";
        protected const string LastOpenDoorTimeStateKey = "LastOpenDoorTimeState";

        protected override async Task<object> CheckMessageForAlarmAsync(DeviceMessage currentDeviceMessage, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "Check Message TemperatureOpenDoorDevice.");
            object alarmMsg = null;
            if (currentDeviceMessage.MessageType == MessagePropertyName.TempOpenDoorType)
            {
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

                    ActorEventSource.Current.ActorMessage(this, "DeviceActor - State has been updated");
                }
            }
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
        public override async Task<DeviceConfigurationData> GetConfigurationAsync(CancellationToken cancellationToken)
        {
            DeviceConfigurationData config = new DeviceConfigurationData();

            await GetConfigurationAsync<double>(DeviceConfigurationPropertyNames.TemperatureThresholdName, config, cancellationToken);
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
        public override async Task<bool> SetConfigurationAsync(DeviceConfigurationData config, CancellationToken cancellationToken)
        {
            if (config == null)
                throw new NullReferenceException(nameof(config));

            bool result = true;

            result &= await SetConfigurationValueAsync<double>(DeviceConfigurationPropertyNames.TemperatureThresholdName, config, cancellationToken);
            result &= await SetConfigurationValueAsync<TimeSpan>(DeviceConfigurationPropertyNames.OpenDoorDutationName, config, cancellationToken);

            return result;
        }
        #endregion [ IDeviceConfiguration methods ]

    }
}
