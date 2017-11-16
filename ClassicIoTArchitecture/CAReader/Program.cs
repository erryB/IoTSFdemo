using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.ServiceBus;

namespace CAReader
{
    class Program
    {
        public static DeviceMsg deviceMsg;
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";


        static void Main(string[] args)
        {
            Console.WriteLine("Reader - reads messages from Q1. Ctrl-C to exit.\n");
            var queueName = "sbqueue1";
            var queueClient = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queueClient.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();
                DateTime timestamp = message.EnqueuedTimeUtc;

                deviceMsg = ValidateMessage(s, timestamp);
                Console.WriteLine(String.Format("validated message: {0}, timestamp {1}", s, timestamp));

                SendToTopic();
                Console.WriteLine(String.Format("message sent to Topic2\n"));
                
            });

            Console.ReadLine();
        }

        public static DeviceMsg ValidateMessage(string messageString, DateTime timestamp)
        {
            var msg = JsonConvert.DeserializeObject(messageString);
            JObject json = JObject.Parse(messageString);

            DeviceMsg deviceMsg = new DeviceMsg
            {
                DeviceID = json["deviceId"].Value<string>(),
                MsgID = json["messageId"].Value<int>(),
                TimeStamp = timestamp,
                Data = messageString
            };

            return deviceMsg;
        }

        public static void SendToTopic()
        {
            var topicName = "sbtopic2";
            var client = TopicClient.CreateFromConnectionString(sbConnectionString, topicName);
            
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(deviceMsg.Data);
            writer.Flush();
            stream.Position = 0;

                       
            var bm = new BrokeredMessage(stream)
            {
                Label = "ValidMessagefrom" + deviceMsg.DeviceID
            };
            bm.Properties["timestamp"] = deviceMsg.TimeStamp;

            client.Send(bm);
            
        }

     
    }
}
