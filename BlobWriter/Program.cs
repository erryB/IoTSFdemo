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

namespace BlobWriter
{
    class Program
    {
        public static string data;
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";



        static void Main(string[] args)
        {
            Console.WriteLine("BlobWriter - reads messages from Q2, writes to Blob. Ctrl-C to exit.\n");
            
            var topicName = "sbtopic2";
            var subscriptionName = "toBlob";
            var client = SubscriptionClient.CreateFromConnectionString(sbConnectionString, topicName, subscriptionName);


            //Parse the connection string for the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

            //Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("ebcontainer");

            //Create the container if it does not already exist.
            container.CreateIfNotExists();

            //Get a reference to an append blob.
            CloudAppendBlob appendBlob = container.GetAppendBlobReference("receivedData.json");

            //Create the append blob. Note that if the blob already exists, the CreateOrReplace() method will overwrite it.
            //You can check whether the blob exists to avoid overwriting it by using CloudAppendBlob.Exists().
            appendBlob.CreateOrReplace();

            client.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                data = reader.ReadToEnd();

                //Console.WriteLine(String.Format("BlobWriter - Message read from Q2: {0}", data));
                appendBlob.AppendText(data + "\n");

                Console.WriteLine("appended line to Blob: {0}", data);
                
            });

            Console.ReadLine();
        }
    }
}
