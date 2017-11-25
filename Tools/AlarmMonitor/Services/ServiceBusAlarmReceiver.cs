using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace AlarmMonitor.Services
{
    public class ServiceBusAlarmReceiver : IAlarmReceiver
    {
        private SubscriptionClient receiverClient;
        private readonly IConfigurationAdapter configAdapter;
        private NamespaceManager namespaceManager;
        private string sbConnectionString;
        private string sbTopicName;
        private string sbTopicSubscriptionName;

        public ServiceBusAlarmReceiver(IConfigurationAdapter configAdapter)
        {
            if (configAdapter == null)
                throw new ArgumentNullException(nameof(configAdapter));

            this.configAdapter = configAdapter;
        }

        public async Task StartReceiverAsync()
        {
            await ReadConfiguration();
            namespaceManager = NamespaceManager.CreateFromConnectionString(this.sbConnectionString);
            if (!(namespaceManager.SubscriptionExists(this.sbTopicName, this.sbTopicSubscriptionName)))
            {
                namespaceManager.CreateSubscription(this.sbTopicName, this.sbTopicSubscriptionName);
            }
            receiverClient = SubscriptionClient.CreateFromConnectionString(this.sbConnectionString, this.sbTopicName, this.sbTopicSubscriptionName);
            receiverClient.OnMessage(MessageReceived);
        }

        private void MessageReceived(BrokeredMessage arg)
        {
            try
            {
                var body = arg.GetBody<Stream>();
                string bodyStr;
                using (var reader = new StreamReader(body, true))
                {
                    bodyStr = reader.ReadToEnd();
                }

                var alarmMessage = JsonConvert.DeserializeObject<AlarmMessage>(bodyStr);
                AlarmMessageReceived?.Invoke(this, alarmMessage);
            }
            catch (Exception ex)
            {
 
            }
        }

        private async Task ReadConfiguration()
        {
            this.sbConnectionString = await configAdapter.GetConfigurationValueAsync<string>("SBConnectionString");
            this.sbTopicName = await configAdapter.GetConfigurationValueAsync<string>("SBTopicName");
            this.sbTopicSubscriptionName = await configAdapter.GetConfigurationValueAsync<string>("SBTopicSubscriptionName");
        }

        public Task StopReceiverAsync()
        {
            return receiverClient.CloseAsync();
        }

        public event EventHandler<AlarmMessage> AlarmMessageReceived;
    }
}
