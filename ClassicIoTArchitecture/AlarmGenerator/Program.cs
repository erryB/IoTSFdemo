using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UsefulResources;
using System.Configuration;

namespace AlarmGenerator 
{
    class Program
    {
        //public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        public static string queueName = "sbqueue3";
        public static string topicName = "sbalarmclassic";

        static void Main(string[] args)
        {
            Console.WriteLine($"AlarmGenerator - reads messages from {queueName} and generates alarms. Ctrl-C to exit.\n");

            var queueClient = QueueClient.CreateFromConnectionString(ConfigurationManager.AppSettings["sbConnectionstring"], queueName);
            string alarm = null;

            queueClient.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();

                DeviceMessage currentMessage = new DeviceMessage(s);
                alarm = "no alarm";

                if (currentMessage.MessageType == MessagePropertyName.TempHumType)
                {
                    if(Convert.ToDouble(currentMessage.MessageData[MessagePropertyName.Temperature]) > AlarmParameters.TemperatureTH && Convert.ToDouble(currentMessage.MessageData[MessagePropertyName.TempIncreasingSec]) > AlarmParameters.IncTempSecTH)
                    {
                        alarm = $"ALARM - Temperature is too high and keeps increasing - {s}";
                    }
                    
                } else if(currentMessage.MessageType == MessagePropertyName.TempOpenDoorType)
                {
                    if (Convert.ToDouble(currentMessage.MessageData[MessagePropertyName.Temperature]) > AlarmParameters.TemperatureTH && Convert.ToDouble(currentMessage.MessageData[MessagePropertyName.OpenDoorSec]) > AlarmParameters.OpenDoorTH)
                    {
                        alarm = $"ALARM - Temperature is too high and the door is open. CLOSE THE DOOR - {s}";
                    }
                } else
                {
                    alarm = "ERROR";
                }

                Console.WriteLine(alarm);

                if(alarm != null && alarm != "no alarm")
                {
                    SendToTopic(currentMessage, s, ConfigurationManager.AppSettings["sbConnectionstring"], topicName);
                }

            });

            Console.ReadLine();
        }

        public static void SendToTopic(DeviceMessage msg, string messageString, string sbConnectionString, string topicName)
        {
            var client = TopicClient.CreateFromConnectionString(sbConnectionString, topicName);

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(messageString);
            writer.Flush();
            stream.Position = 0;


            var bm = new BrokeredMessage(stream)
            {
                Label = "ValidMessagefrom" + msg.DeviceID
            };
            bm.Properties["type"] = msg.MessageType;

            client.Send(bm);

        }


    }
}
