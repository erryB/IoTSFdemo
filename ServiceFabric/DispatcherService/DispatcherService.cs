using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DeviceActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using BlobWriterService;
using BlobWriter.interfaces;

using System.Runtime.Serialization;
using CommonResources;

namespace DispatcherService
{
    
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DispatcherService : StatelessService 
    {
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        public string deviceID = null;

        public DispatcherService(StatelessServiceContext context)
            : base(context)
        { }

        
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
            
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var queueName = "sbqueue1";
                var queueClient = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

                queueClient.OnMessage(async message =>
                {
                    Stream stream = message.GetBody<Stream>();
                    StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                    string s = await reader.ReadToEndAsync();
                    DateTime timestamp = message.EnqueuedTimeUtc;

                    var deviceMsg = new DeviceMessage(s, timestamp);
                    
                    var proxyActor = ActorProxy.Create<IDeviceActor>(new ActorId(deviceMsg.DeviceID), new Uri("fabric:/EBIoTApplication/DeviceActor"));
                    var proxyBlob = ServiceProxy.Create<IBlobWriterService>(new Uri("fabric:/EBIoTApplication/BlobWriterService"));

                    //parallel execution of 2 independent tasks
                    await Task.WhenAll(proxyActor.UpdateDeviceStateAsync(deviceMsg, cancellationToken), proxyBlob.ReceiveMessageAsync(deviceMsg, cancellationToken));


                });
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }
    }
}
