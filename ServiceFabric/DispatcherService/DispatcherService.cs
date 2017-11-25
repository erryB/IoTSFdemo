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

namespace DispatcherService
{
    
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DispatcherService : StatelessService
    {
        private string sbConnectionString;
        private string queueName;

        public DispatcherService(StatelessServiceContext context)
            : base(context)
        { }

        private QueueClient queueClient;
        
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
            
        }
        

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - running");
            this.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            this.ReadSettings();
            CreateQueueClient(cancellationToken);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();


                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }

        private void CreateQueueClient(CancellationToken cancellationToken)
        {
            queueClient = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queueClient.OnMessage(async message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = await reader.ReadToEndAsync();
                DateTime timestamp = message.EnqueuedTimeUtc;

                //debug
                ServiceEventSource.Current.ServiceMessage(this.Context, $"DispatcherService - received message");

                var deviceMsg = new DeviceMessage(s, timestamp);

                //debug
                ServiceEventSource.Current.ServiceMessage(this.Context, $"DispatcherService - DeviceMessage created. DeviceID {deviceMsg.DeviceID}");

                var proxyActor = ActorProxy.Create<IDeviceActor>(new ActorId(deviceMsg.DeviceID), new Uri("fabric:/EBIoTApplication/DeviceActor"));
                var proxyBlob = ServiceProxy.Create<IBlobWriterService>(new Uri("fabric:/EBIoTApplication/BlobWriterService"));

                //debug
                ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - ProxyActor and ProxyBlob created");

                await proxyBlob.ReceiveMessageAsync(deviceMsg, cancellationToken);

                //await proxyActor.UpdateDeviceStateAsync(deviceMsg, cancellationToken);

                //parallel execution of 2 independent tasks
                //await Task.WhenAll(proxyActor.UpdateDeviceStateAsync(deviceMsg, cancellationToken), proxyBlob.ReceiveMessageAsync(deviceMsg, cancellationToken));

                //debug
                ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - message sent to BlobWriter");


            });
        }

        private void ReadSettings()
        {
            ConfigurationSettings settingsFile = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings;

            ConfigurationSection configSection = settingsFile.Sections["Connections"];

            this.sbConnectionString = configSection.Parameters["ServiceBusConnectionString"].Value;
            this.queueName = configSection.Parameters["ServiceBusQueueName"].Value;
        }

        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            this.ReadSettings();
            if(queueClient != null)
            {
                
            }
            CreateQueueClient(default(CancellationToken));

        }


    }
}
