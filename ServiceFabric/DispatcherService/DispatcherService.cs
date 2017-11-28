using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Text;
using DeviceActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using BlobWriter.interfaces;
using CommonResources;
using System.Fabric.Description;
using System.Linq;

namespace DispatcherService
{

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DispatcherService : StatefulService
    {
        private static readonly IDictionary<string, string> DeviceMessageMap = new Dictionary<string, string>()
        {
            {MessagePropertyName.TempOpenDoorType, "fabric:/EBIoTApplication/TDDeviceActor"},
            {MessagePropertyName.TempHumType, "fabric:/EBIoTApplication/THDeviceActor"}
        };

        private string sbConnectionString;
        private string queueName;

        public DispatcherService(StatefulServiceContext context)
            : base(context)
        { }

        private QueueClient queueClient;

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - Running");
            this.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            this.ReadSettings();
            await CreateQueueClientAsync(cancellationToken);

            //await CreateHUbClientAsync(cancellationToken);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //EventData eventData = await eventHubReceiver.ReceiveAsync();
                //if (eventData != null)
                //{
                //    await ElaborateEventDataAsync(eventData, cancellationToken);
                //}
                //await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }

        #region [ EventHub integration]
        private EventHubClient eventHubClient;
        private EventHubReceiver eventHubReceiver;

        private async Task CreateHUbClientAsync(CancellationToken cancellationToken)
        {
            if (eventHubReceiver != null)
                await eventHubReceiver.CloseAsync();
            if (eventHubClient != null)
                await eventHubClient.CloseAsync();

            eventHubClient = EventHubClient.CreateFromConnectionString(sbConnectionString, "messages/events");

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            var servicePartitionKey = ((Int64RangePartitionInformation)this.Partition.PartitionInfo).LowKey;

            var hubPartition = d2cPartitions.FirstOrDefault(p => p == servicePartitionKey.ToString());
            if (hubPartition != null)
            {
                eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(hubPartition, DateTime.UtcNow);
            }
        }

        private async Task ElaborateEventDataAsync(EventData eventData, CancellationToken cancellationToken)
        {
            string s = Encoding.UTF8.GetString(eventData.GetBytes());

            var deviceMsg = new DeviceMessage(s, eventData.EnqueuedTimeUtc);
            if (DeviceMessageMap.ContainsKey(deviceMsg.MessageType))
            {
                var proxyActor = ActorProxy.Create<IDeviceActor>(new ActorId(deviceMsg.DeviceID), new Uri(DeviceMessageMap[deviceMsg.MessageType]));
                await proxyActor.UpdateDeviceStateAsync(deviceMsg, cancellationToken);
                ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - message sent to DeviceActor");
            }

            var proxyBlob = ServiceProxy.Create<IBlobWriterService>(new Uri("fabric:/EBIoTApplication/BlobWriterService"));
            await proxyBlob.ReceiveMessageAsync(deviceMsg, cancellationToken);
            ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - message sent to BlobWriter");
        }
        #endregion [ EventHub integration ]

        #region [ ServiceBusQueue integration ]
        private async Task CreateQueueClientAsync(CancellationToken cancellationToken)
        {
            if (queueClient != null)
                await queueClient.CloseAsync();

            queueClient = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queueClient.OnMessage(async message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = await reader.ReadToEndAsync();
                DateTime timestamp = message.EnqueuedTimeUtc;

                var deviceMsg = new DeviceMessage(s, timestamp);
                if (DeviceMessageMap.ContainsKey(deviceMsg.MessageType))
                {
                    var proxyActor = ActorProxy.Create<IDeviceActor>(new ActorId(deviceMsg.DeviceID), new Uri(DeviceMessageMap[deviceMsg.MessageType]));
                    try
                    {
                        await proxyActor.UpdateDeviceStateAsync(deviceMsg, cancellationToken);
                        ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - message sent to DeviceActor");
                    }
                    catch (Exception ex)
                    {
                        ServiceEventSource.Current.ServiceMessage(this.Context, "[EXCEPTION] {0}", ex);
                    }
                }

                var proxyBlob = ServiceProxy.Create<IBlobWriterService>(new Uri("fabric:/EBIoTApplication/BlobWriterService"));
                try
                {
                    await proxyBlob.ReceiveMessageAsync(deviceMsg, cancellationToken);
                    ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - message sent to BlobWriter");
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, "[EXCEPTION] {0}", ex);
                }
            });
        }
        #endregion [ ServiceBusQueue integration ]


        private void ReadSettings()
        {
            ConfigurationSettings settingsFile = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings;

            ConfigurationSection configSection = settingsFile.Sections["Connections"];

            this.sbConnectionString = configSection.Parameters["ServiceBusConnectionString"].Value;
            this.queueName = configSection.Parameters["ServiceBusQueueName"].Value;
        }

        private async void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            this.ReadSettings();

            await CreateQueueClientAsync(default(CancellationToken));
            //await CreateHUbClientAsync(default(CancellationToken));

        }


    }
}
