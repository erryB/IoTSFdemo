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
        //public static Stream stream;

        static void Main(string[] args)
        {
            Console.WriteLine("Reader - reads messages from Q1. Ctrl-C to exit.\n");
            var connectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
            var queueName = "sbqueue2";
            var queueClient = QueueClient.CreateFromConnectionString(connectionString, queueName);

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

            queueClient.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                data = reader.ReadToEnd();

                //Console.WriteLine(String.Format("BlobWriter - Message read from Q2: {0}", data));
                appendBlob.AppendText(data);

                //Read the append blob to the console window.
                //Console.WriteLine(appendBlob.DownloadText());

            });

            

            //int numBlocks = 10;

            ////Generate an array of random bytes.
            //Random rnd = new Random();
            //byte[] bytes = new byte[numBlocks];
            //rnd.NextBytes(bytes);

            ////Simulate a logging operation by writing text data and byte data to the end of the append blob.
            //for (int i = 0; i < numBlocks; i++)
            //{
            //    appendBlob.AppendText(String.Format("Timestamp: {0:u} \tLog Entry: {1}{2}",
            //        DateTime.UtcNow, bytes[i], Environment.NewLine));
            //}


            

            Console.ReadLine();
        }
    }
}
