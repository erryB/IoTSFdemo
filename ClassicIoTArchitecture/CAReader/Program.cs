using Microsoft.ServiceBus.Messaging;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UsefulResources;

namespace CAReader
{
    class Program
    {
        public static DeviceMessage deviceMsg;
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        public static string queueName = "sbqueue1";
        public static string topicName = "sbtopic2";

        static void Main(string[] args)
        {
            Console.WriteLine($"CAReader - reads messages from {queueName}. Ctrl-C to exit.\n");

            var queueClient = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queueClient.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();
                DateTime timestamp = message.EnqueuedTimeUtc;

                try
                {
                    //identification of the proper MessageType
                    deviceMsg = new DeviceMessage(s, timestamp);
                    Console.WriteLine($"validated message: {s}, timestamp {timestamp}");

                    SendToTopic(JsonConvert.SerializeObject(deviceMsg), sbConnectionString, topicName);
                    Console.WriteLine($"message sent to {topicName}\n");

                }
                catch (Exception)
                {
                    Console.WriteLine("Parsing error");
                }

            });

            Console.ReadLine();
        }



        public static void SendToTopic(string messageString, string sbConnectionString, string topicName)
        {
            var client = TopicClient.CreateFromConnectionString(sbConnectionString, topicName);

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(messageString);
            writer.Flush();
            stream.Position = 0;


            var bm = new BrokeredMessage(stream)
            {
                Label = "ValidMessagefrom" + deviceMsg.DeviceID
            };
            bm.Properties["type"] = deviceMsg.MessageType;

            client.Send(bm);

        }


    }
}
