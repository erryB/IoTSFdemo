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
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            //long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "DispatcherService - running");
                var queueName = "sbqueue1";
                var queueClient = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

                queueClient.OnMessage(async message =>
                {
                    Stream stream = message.GetBody<Stream>();
                    StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                    string s = await reader.ReadToEndAsync();
                    DateTime timestamp = message.EnqueuedTimeUtc;

                    string messageToBeSent = validateMessage(s, timestamp);
                    //Console.WriteLine(String.Format("validated message: {0}, timestamp {1}", s, timestamp));

                    //sendToTopic();
                    //Console.WriteLine(String.Format("message sent to Topic2\n"));

                    SendToBatmanDeviceActor(messageToBeSent);
                    SendToBlobWriterService(messageToBeSent);

                });

                //Console.ReadLine();
            }

            string validateMessage(string messageString, DateTime ts)
            {
                string returnMessage = null;
                var msg = JsonConvert.DeserializeObject(messageString);
                JObject json = JObject.Parse(messageString);
                string deviceID = json["deviceId"].Value<string>();


                if ( deviceID == "Batman")
                {
                    
                    try
                    {
                       
                        var messageToBeSent = new
                        {
                            deviceID = "Batman",
                            messageID = json["messageId"].Value<int>(),
                            timestamp = ts,
                            temperature = json["temperature"].Value<double>(),
                            humidity = json["humidity"].Value<double>()                           
                        };
                        returnMessage = JsonConvert.SerializeObject(messageToBeSent);
                        

                    } catch (Exception e)
                    {
                        returnMessage = "Error in parsing message from Batman";
                    }
                   
                } else if(deviceID == "Joker")
                {
                    try
                    {

                        var messageToBeSent = new
                        {
                            deviceID = "Joker",
                            messageID = json["messageId"].Value<int>(),
                            timestamp = ts,
                            temperature = json["temperature"].Value<double>(),
                            doorOpen = json["doorOpen"].Value<double>()
                        };
                        returnMessage = JsonConvert.SerializeObject(messageToBeSent);


                    }
                    catch (Exception e)
                    {
                        returnMessage = "Error in parsing message from Joker";
                    }
                }
                return returnMessage;

            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
