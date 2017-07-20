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
using CommonResources;
using System.Runtime.Serialization;

namespace DispatcherService
{
    //public interface IDispatcherService : IService
    //{
    //    Task<string> HelloWorldAsync();
    //}


    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DispatcherService : StatelessService /*IDispatcherService*/
    {
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        public string deviceID = null;

        public DispatcherService(StatelessServiceContext context)
            : base(context)
        { }

        //public Task<string> HelloWorldAsync()
        //{
        //    return Task.FromResult("Hello from DispatcherService!");
        //}

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
            //return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };
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

                    var device = validateMessage(s, timestamp);
                    string messageToBeSent = Serialize(device);//ok???

                    var proxyActor = ActorProxy.Create<IDeviceActor>(new ActorId(deviceID), new Uri("fabric:/EBIoTApplication/DeviceActor"));
                    await proxyActor.UpdateDeviceState(device);//not implemented yet

                    var proxyBlob = ServiceProxy.Create<IBlobWriterService>(new Uri("fabric:/EBIoTApplication/BlobWriterService"));
                    await proxyBlob.ReceiveMessageAsync(messageToBeSent);

                });
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);



            }

            Device validateMessage(string messageString, DateTime ts)
            {
                Device returnDevice;

                var msg = JsonConvert.DeserializeObject(messageString);
                JObject json = JObject.Parse(messageString);
                var deviceID = json["deviceId"].Value<string>();

                if(deviceID == "Batman")
                {
                    var batmanDevice = new Batman();

                    batmanDevice.Humidity = json["humidity"].Value<double>();
                    batmanDevice.Temperature = json["temperature"].Value<double>();
                    returnDevice = batmanDevice;

                } else if (deviceID == "Joker")
                {
                    var jokerDevice = new Joker();
                    jokerDevice.Temperature = json["temperature"].Value<double>();
                    jokerDevice.OpenDoor = json["doorOpen"].Value<bool>();
                    returnDevice = jokerDevice;
                }
                else
                {
                    returnDevice = new Device();
                }

                returnDevice.DeviceID = deviceID;
                returnDevice.MessageID = json["messageId"].Value<int>();
                returnDevice.Timestamp = ts;


                return returnDevice;

            }

            string Serialize(object obj)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                    serializer.WriteObject(memoryStream, obj);
                    memoryStream.Position = 0;
                    return reader.ReadToEnd();
                }
            }



        }
    }
}
