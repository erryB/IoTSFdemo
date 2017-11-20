using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace ReadQueue
{
    class Program
    {
        static string queueName = "sbqueue1";
        static string connectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=iothubroutes_EBIoTHubDemo;SharedAccessKey=kesA6tP8p7LkWsiDCoYSRsEgxSvcx07LYZJLoLiwQLk=;EntityPath=sbqueue1";

        static void Main(string[] args)
        {
            Console.WriteLine($"Reading {queueName}. Ctrl-C to exit.\n");

            var client = QueueClient.CreateFromConnectionString(connectionString);

            client.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();
                Console.WriteLine(String.Format("Message Label {0},\n body: {1}", message.Label, s));
                
                
            });

            Console.ReadLine();
        }
    }
}
