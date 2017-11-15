using System;
using System.Collections.Generic;
using System.Fabric;
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
            ReliableQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<string>>("myReliableQueue");

            //enqueue current alarm message to ReliableQueue, timeout 5 sec
            using (var tx = this.StateManager.CreateTransaction())
            {
                var timeout = new System.TimeSpan(0, 0, 5);
                await ReliableQueue.EnqueueAsync(tx, alarmMessage, timeout, cancellationToken);
                await tx.CommitAsync();
            }
            
            //update the state???
        }

        public Task SendToTopic(string alarmMessage, string topicName, CancellationToken cancellationToken)
        {
            var client = TopicClient.CreateFromConnectionString(sbConnectionString, topicName);

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(alarmMessage);
            writer.Flush();
            stream.Position = 0;


            var bm = new BrokeredMessage(stream)
            {
                Label = "ALARM MESSAGE"
            };
            
            client.Send(bm);

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
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            
            ReliableQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<string>>("myReliableQueue");
            sbConnectionString = "59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
            topicName = "sbalarmtopic";

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var itemFromQueue = await ReliableQueue.TryDequeueAsync(tx).ConfigureAwait(false);
                    if(!itemFromQueue.HasValue)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    
                    JObject json = JObject.Parse(itemFromQueue.Value);                    
                    var deviceID = json["DeviceID"].Value<string>();
                    var msgID = json["MessageID"].Value<int>();
                    var timestamp = json["Timestamp"].Value<DateTime>();
                    var msg = json["AlarmMessage"].Value<string>();

                    string line = $"{deviceID} - MessageID {msgID}: {msg} - timestamp: {timestamp}";

                    //debug print
                    ServiceEventSource.Current.ServiceMessage(this.Context, line);

                    SendToTopic(line, topicName, cancellationToken);

                    //update the state???
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            
            
        }
    }
}
