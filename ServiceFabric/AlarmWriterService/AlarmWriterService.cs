using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using AlarmWriterService.Interfaces;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace AlarmWriterService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class AlarmWriterService : StatefulService, IAlarmWriterService
    {

        IReliableQueue<string> ReliableQueue;
        string sbConnectionString;
        string topicName;

        public AlarmWriterService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task SendAlarmAsync(string alarmMessage, CancellationToken cancellationToken)
        {
            //enqueue current alarm message to ReliableQueue, timeout 5 sec
            using (var tx = this.StateManager.CreateTransaction())
            {
                await ReliableQueue.EnqueueAsync(tx, alarmMessage, TimeSpan.FromSeconds(5), cancellationToken);
                await tx.CommitAsync();
            }

        }

        private void ReadSettings()
        {
            ConfigurationSettings settingsFile = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings;

            ConfigurationSection configSection = settingsFile.Sections["Connections"];

            this.sbConnectionString = configSection.Parameters["ServiceBusConnectionString"].Value;
            this.topicName = configSection.Parameters["ServiceBusTopicName"].Value;
        }

        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            this.ReadSettings();
        }

        public async Task<bool> SendToTopic(string alarmMessage, CancellationToken cancellationToken)
        {
            bool retVal = false;

            try
            {
                var client = TopicClient.CreateFromConnectionString(sbConnectionString, topicName);

                using (MemoryStream stream = new MemoryStream())
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(alarmMessage);
                    writer.Flush();
                    stream.Position = 0;

                    var bm = new BrokeredMessage(stream)
                    {
                        Label = "ALARM MESSAGE"
                    };

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Sending Alarm to topic - {0}", alarmMessage);
                    await client.SendAsync(bm);
                    ServiceEventSource.Current.ServiceMessage(this.Context, "Sent Alarm to topic - {0}", alarmMessage);
                    retVal = true;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "[EXCEPTION] {0}", ex);
            }
            return retVal;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(this.CreateServiceRemotingListener) };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            this.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            this.ReadSettings();

            ReliableQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<string>>("myReliableQueue");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ConditionalValue<string> itemFromQueue; //diventa alarm message

                using (var tx = this.StateManager.CreateTransaction())
                {
                    itemFromQueue = await ReliableQueue.TryPeekAsync(tx);
                    await tx.CommitAsync();
                }
                if (itemFromQueue.HasValue)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, "Alarm message in queue - {0}", itemFromQueue.Value);

                    if (await SendToTopic(itemFromQueue.Value, cancellationToken))
                        using (var tx = this.StateManager.CreateTransaction())
                        {
                            await ReliableQueue.TryDequeueAsync(tx);
                            await tx.CommitAsync();
                        }

                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }



        }
    }
}
