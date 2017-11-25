using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;

namespace ReadQueue
{
    class Program
    {
        static string queueName = "sbqueue1";

        static void Main(string[] args)
        {
            Console.WriteLine($"Reading {queueName}. Ctrl-C to exit.\n");

            var client = QueueClient.CreateFromConnectionString(ConfigurationManager.AppSettings["connectionString"]);

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
