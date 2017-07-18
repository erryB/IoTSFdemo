using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceBus.Messaging;
using System.IO;

namespace AlarmService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class AlarmService : StatelessService
    {
        public string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        public string alarm; //received from DeviceActor
        public string actor;

        public AlarmService(StatelessServiceContext context)
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
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            //long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);
                if(alarm != null && actor != null)
                {                   
                    sendToTopic(alarm, actor);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public void sendToTopic(string message, string deviceId)
        {
            var topicName = "sbalarmtopic";
            var client = TopicClient.CreateFromConnectionString(sbConnectionString, topicName);

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(message);
            writer.Flush();
            stream.Position = 0;


            var bm = new BrokeredMessage(stream)
            {
                Label = "ALARM from " + deviceId
            };
            //bm.Properties["timestamp"] = deviceMsg.TimeStamp;

            client.Send(bm);

        }
    }
}
