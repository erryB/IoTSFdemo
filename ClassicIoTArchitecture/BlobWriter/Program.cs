using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Newtonsoft.Json;
using System.Configuration;

namespace BlobWriter
{
    class Program
    {

        public static string topicName = "sbtopic2";
        public static string subscriptionName = "toBlob";
        public static string containerName = "ebcontainer";

        static void Main(string[] args)
        {
            Console.WriteLine($"BlobWriter - reads messages from {topicName}, writes to Blob. Ctrl-C to exit.\n");
           
            var client = SubscriptionClient.CreateFromConnectionString(ConfigurationManager.AppSettings["sbConnectionstring"], topicName, subscriptionName);


            //Parse the connection string for the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

            //Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            //Create the container if it does not already exist.
            container.CreateIfNotExists();

            //Get a reference to an append blob.
            CloudAppendBlob appendBlob = container.GetAppendBlobReference("ClassicIoTBlobWriter.json");

            //Create the append blob. Note that if the blob already exists, the CreateOrReplace() method will overwrite it.
            //You can check whether the blob exists to avoid overwriting it by using CloudAppendBlob.Exists().
            appendBlob.CreateOrReplace();

            //Console.WriteLine("Waiting for a message");

            client.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string data = reader.ReadToEnd();

                //Console.WriteLine(String.Format("BlobWriter - Message read from Q2: {0}", data));
                appendBlob.AppendText(data + "\n");

                Console.WriteLine("appended line to Blob: {0}", data);
                
            });

            //Console.WriteLine("no messages");

            Console.ReadLine();
        }
    }
}
