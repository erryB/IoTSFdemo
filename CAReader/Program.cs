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
        static void Main(string[] args)
        {
            Console.WriteLine("Reader - reads messages from Q1. Ctrl-C to exit.\n");
            var connectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
            var queueName = "sbqueue1";

            var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

            client.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();

                var msg = JsonConvert.DeserializeObject(s);
                JObject json = JObject.Parse(s);

                DeviceMsg deviceMsg = new DeviceMsg();

                deviceMsg.DeviceID = json["deviceId"].Value<string>();
                deviceMsg.MsgID = json["messageId"].Value<int>();
                deviceMsg.TimeStamp = message.EnqueuedTimeUtc;
                deviceMsg.Data = msg;
            
                Console.WriteLine(String.Format("Message body: {0}", s));
            });

            Console.ReadLine();
        }
    }
}
