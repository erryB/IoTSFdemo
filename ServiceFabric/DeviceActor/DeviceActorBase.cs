using System;
using System.Threading;
using System.Threading.Tasks;
using AlarmWriterService.Interfaces;
using CommonResources;
using DeviceActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Newtonsoft.Json;

namespace DeviceActor
{
    public abstract class DeviceActorBase : Actor,  IRemindable
    {
        protected DeviceActorBase(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }

        protected const string LastDeviceMassageStateKey = "LastDeviceMessageState";
        protected const string SendAlarmMessageReminderName = "SendAlarmMessageReminder";
        protected const string SendAlarmMessageQueueName = "SendAlarmMessageQueue";
        protected const int SendAlarmMessageReminderDueTimeInMSec = 100;

        public async Task UpdateDeviceStateAsync(DeviceMessage currentDeviceMessage, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "Update message arrived. {0}", currentDeviceMessage.MessageType);
            object alarmMsg = null;

            var lastDeviceMessage = await this.StateManager.GetOrAddStateAsync<DeviceMessage>(LastDeviceMassageStateKey, null, cancellationToken);
            if (lastDeviceMessage == null || lastDeviceMessage.Timestamp < currentDeviceMessage.Timestamp) // drop automatically the oldest messages from the last arrived
            {
                await this.StateManager.SetStateAsync<DeviceMessage>(LastDeviceMassageStateKey, currentDeviceMessage, cancellationToken);
                alarmMsg = await CheckMessageForAlarmAsync(currentDeviceMessage, cancellationToken);

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
                ActorEventSource.Current.ActorMessage(this, $"Reminder {0} received.", SendAlarmMessageReminderName);
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
                        ActorEventSource.Current.ActorMessage(this, "[EXCEPTION] {0}", ex);
                    }
                }
                if (await this.StateManager.GetQueueLengthAsync(SendAlarmMessageQueueName) > 0)
                    await RegisterReminderAsync(SendAlarmMessageReminderName, null, TimeSpan.FromMilliseconds(SendAlarmMessageReminderDueTimeInMSec), TimeSpan.FromMilliseconds(-1));

            }
        }

        protected abstract Task<object> CheckMessageForAlarmAsync(DeviceMessage currentDeviceMessage, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the configuration asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;DeviceConfigurationData&gt;.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public abstract Task<DeviceConfigurationData> GetConfigurationAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sets the configuration asynchronous.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public abstract Task<bool> SetConfigurationAsync(DeviceConfigurationData config, CancellationToken cancellationToken);

        #region [ Private Methods ]

        protected async Task<bool> SetConfigurationValueAsync<T>(string configurationName, DeviceConfigurationData config, CancellationToken cancellationToken)
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

        protected async Task GetConfigurationAsync<T>(string configurationName, DeviceConfigurationData config, CancellationToken cancellationToken)
        {
            var value = await this.StateManager.TryGetStateAsync<T>(configurationName, cancellationToken);
            if (value.HasValue)
                config.Properties[configurationName] = value.Value;
        }
        #endregion [ Private Methods ]
    }
}