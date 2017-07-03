using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CAReader
{
    class Program
    {
        public static DeviceMsg deviceMsg;
        public static string connectionString;


        static void Main(string[] args)
        {
            Console.WriteLine("Reader - reads messages from Q1. Ctrl-C to exit.\n");
            connectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
            var queueName = "sbqueue1";
            var queueClient = QueueClient.CreateFromConnectionString(connectionString, queueName);

            queueClient.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();
                DateTime timestamp = message.EnqueuedTimeUtc;

                deviceMsg = validateMessage(s, timestamp);
                sendToQueue();

                Console.WriteLine(String.Format("validated message: {0}, timestamp {1}\nsent to Q2", s, timestamp));
                
            });

            Console.ReadLine();
        }

        public static DeviceMsg validateMessage(string messageString, DateTime timestamp)
        {
            var msg = JsonConvert.DeserializeObject(messageString);
            JObject json = JObject.Parse(messageString);

            DeviceMsg deviceMsg = new DeviceMsg();

            deviceMsg.DeviceID = json["deviceId"].Value<string>();
            deviceMsg.MsgID = json["messageId"].Value<int>();
            deviceMsg.TimeStamp = timestamp;
            deviceMsg.Data = messageString;

            return deviceMsg;
        }

        public static void sendToQueue()
        {
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

            var queueName = "sbqueue2";
            var queue2Client = QueueClient.CreateFromConnectionString(connectionString, queueName);

            queue2Client.Send(bm);


        }

        public static void sendToBlobWriter()
        {

        }
        public static void sendToNewStateCalculator()
        {

        }
    }
}
