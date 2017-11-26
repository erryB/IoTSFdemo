using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using System.Collections;
using System.Collections.Concurrent;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using BlobWriter.interfaces;
using CommonResources;

namespace BlobWriterService
{


    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class BlobWriterService : StatelessService, IBlobWriterService
    {
        //private ConcurrentQueue<DeviceMessage> internalBlobQueue;
        private Queue<DeviceMessage> internalBlobQueue;
        private CloudAppendBlob appendBlob;

        public BlobWriterService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };
        }


        private void ReadSettings()
        {
            ConfigurationSettings settingsFile = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings;

            ConfigurationSection configSection = settingsFile.Sections["Connections"];

            this.storageConnectionString = configSection.Parameters["StorageConnectionString"].Value;
            this.storageContainerName = configSection.Parameters["StorageContainerName"].Value;
            this.logBlobName = configSection.Parameters["LogBlobName"].Value;
        }

        private async void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            this.ReadSettings();
            await CreateBlob();
        }

        public async Task CreateBlob()
        {
            //Parse the connection string for the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.storageConnectionString);

            //Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(this.storageContainerName);

            //Create the container if it does not already exist.
            container.CreateIfNotExists();

            //Get a reference to an append blob.
            appendBlob = container.GetAppendBlobReference(this.logBlobName);

            //Create the append blob. Note that if the blob already exists, the CreateOrReplace() method will overwrite it.
            //You can check whether the blob exists to avoid overwriting it by using CloudAppendBlob.Exists().
            await appendBlob.CreateOrReplaceAsync();
        }

        private string storageConnectionString;
        private string storageContainerName;
        private string logBlobName;

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            //debug
            ServiceEventSource.Current.ServiceMessage(this.Context, "BlobWriterService - Running");

            this.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            this.ReadSettings();

            internalBlobQueue = new Queue<DeviceMessage>();
            await CreateBlob();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (internalBlobQueue.Count != 0)
                {                   
                    DeviceMessage currentMsg = null;
                    if(internalBlobQueue.Count > 0)
                    {
                        currentMsg = internalBlobQueue.Dequeue();
                        appendBlob.AppendText(Convert.ToString(currentMsg)+'\n');
                        
                        ServiceEventSource.Current.ServiceMessage(this.Context, "BlobWriterService - Message to Blob: {0}", currentMsg);
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }


        public Task ReceiveMessageAsync(DeviceMessage message, CancellationToken cancellationToken)
        {           
            if (internalBlobQueue == null)
            {
                internalBlobQueue = new Queue<DeviceMessage>();
            }
            internalBlobQueue.Enqueue(message);
            
            //debug
            ServiceEventSource.Current.ServiceMessage(this.Context, $"BlobWriterService - Enqueued message {message}");
           

            return Task.Delay(0, cancellationToken);
        }
    }
}
