using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using AlarmServiceInterfaces;

namespace AlarmService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class AlarmService : StatelessService, IAlarmService
    {
        private string sbConnectionString;
        private string topicName;

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
            this.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            this.ReadSettings();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }

            this.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent -= CodePackageActivationContext_ConfigurationPackageModifiedEvent;
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

        public Task SendToTopicAsync(string message, string deviceId)
        {

            var client = TopicClient.CreateFromConnectionString(sbConnectionString, topicName);

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteAsync(message);
            writer.Flush();
            stream.Position = 0;


            var bm = new BrokeredMessage(stream)
            {
                Label = "ALARM from " + deviceId
            };

            return client.SendAsync(bm);

        }

        public Task ReceiveAlarmAsync(string alarmMessage, string device)
        {
            return SendToTopicAsync(alarmMessage, device);
        }
    }
}
