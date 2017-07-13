using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Text;
using System.Collections;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace BlobWriterService
{

    public interface IBlobWriterService : IService
    {
        Task<string> receiveMessageAsync(string message);
    }

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class BlobWriterService : StatelessService, IBlobWriterService
    {
        public Queue internalBlobQueue;
        CloudAppendBlob appendBlob;

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


        public void CreateBlob(string containerName, string blobName)
        {
            //Parse the connection string for the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

            //Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            //Create the container if it does not already exist.
            container.CreateIfNotExists();

            //Get a reference to an append blob.
            appendBlob = container.GetAppendBlobReference(blobName);

            //Create the append blob. Note that if the blob already exists, the CreateOrReplace() method will overwrite it.
            //You can check whether the blob exists to avoid overwriting it by using CloudAppendBlob.Exists().
            appendBlob.CreateOrReplace();
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            
            internalBlobQueue = new Queue();

            CreateBlob("ebcontainer", "receivedData.json");
            string currentMsg = null;
            
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if(internalBlobQueue.Count != 0)
                {
                    currentMsg = (string) internalBlobQueue.Dequeue();
                    appendBlob.AppendText(currentMsg + "\n");
                    ServiceEventSource.Current.ServiceMessage(this.Context, "Message to Blob: {0}", currentMsg);
                }
                
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public Task<string> receiveMessageAsync(string message)
        {
           
            if(internalBlobQueue == null)
            {
                internalBlobQueue = new Queue();
            } 
            internalBlobQueue.Enqueue(message);
            return Task.FromResult(message);

        }
    }
}
