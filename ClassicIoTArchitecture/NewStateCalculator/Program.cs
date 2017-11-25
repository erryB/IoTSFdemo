using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulResources;

namespace NewStateCalculator
{
    class Program
    {
        
        //public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        public static string topicName = "sbtopic2";
        public static string subscriptionName = "toNewState";
        public static string queueName = "sbqueue3";

        
        //key: DeviceType
        public static Dictionary<string, Dictionary<string, List<DeviceMessage>>> DeviceTypesStatus;
       

        static void Main(string[] args)
        {
            Console.WriteLine($"NewStateCalculator - reads messages from {topicName}, calculates new Status for each device. Ctrl-C to exit.\n");
            
            var client = SubscriptionClient.CreateFromConnectionString(ConfigurationManager.AppSettings["sbConnectionstring"], topicName, subscriptionName);

            //init dictionary. If you have more DeviceTypes you can use a List
            DeviceTypesStatus = new Dictionary<string, Dictionary<string, List<DeviceMessage>>>
            {
                {
                    MessagePropertyName.TempHumType,
                    //key:DeviceID
                    new Dictionary<string, List<DeviceMessage>>()
                },
                {
                    MessagePropertyName.TempOpenDoorType,
                    new Dictionary<string, List<DeviceMessage>>()
                },
                {
                    MessagePropertyName.UnknownType,
                    new Dictionary<string, List<DeviceMessage>>()
                }
            };

            //readfromtopic  
            client.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();

                Console.WriteLine("received message from topic: {0}", s);

                //create a DeviceMessage from the string read from the topic
                DeviceMessage receivedMessage = new DeviceMessage(s);

                DeviceMessage updatedStatus;

                if(receivedMessage.MessageType == MessagePropertyName.TempHumType)
                {
                    updatedStatus = UpdateTempHumType(receivedMessage);
                }
                else if (receivedMessage.MessageType == MessagePropertyName.TempOpenDoorType)
                {
                    updatedStatus = UpdateTempOpenDoorType(receivedMessage);
                }
                else
                {
                    updatedStatus = new DeviceMessage(MessagePropertyName.UnknownType, MessagePropertyName.UnknownType);
                    
                }

                

                //send to Q
                var StatusToSend = JsonConvert.SerializeObject(updatedStatus);
                //Console.WriteLine($"STATUS TO SEND: {StatusToSend}");
                SendToQueue(StatusToSend, queueName, ConfigurationManager.AppSettings["sbConnectionstring"], updatedStatus);
                Console.WriteLine($"STATUS SENT to {queueName}: {StatusToSend}");

                //update dictionary

                //if the currentDeviceID does not exist in the dictionary, then add it. Otherwise update with a new item on its list
                if (!DeviceTypesStatus[updatedStatus.MessageType].ContainsKey(updatedStatus.DeviceID))
                {
                    DeviceTypesStatus[updatedStatus.MessageType].Add(updatedStatus.DeviceID, new List<DeviceMessage> { updatedStatus });
                }
                else
                {
                    DeviceTypesStatus[updatedStatus.MessageType][updatedStatus.DeviceID].Add(updatedStatus);
                }
                
            });
            Console.ReadLine();
            
        }

        public static DeviceMessage UpdateTempHumType(DeviceMessage currentStatus)
        {
            //if there are no messages from that DeviceID, then create
            if (!DeviceTypesStatus[currentStatus.MessageType].ContainsKey(currentStatus.DeviceID))
            {
                currentStatus.MessageData.Add(MessagePropertyName.TempIncreasingSec, "0");
                currentStatus.MessageData.Add(MessagePropertyName.HumIncreasingSec, "0");
            } else
            {
                var prevDeviceStatus = (DeviceTypesStatus[currentStatus.MessageType][currentStatus.DeviceID]).Last();

                //increasing temperature
                if (Convert.ToDouble(currentStatus.MessageData[MessagePropertyName.Temperature]) > Convert.ToDouble(prevDeviceStatus.MessageData[MessagePropertyName.Temperature]))
                {
                    var DiffSeconds = (currentStatus.Timestamp - prevDeviceStatus.Timestamp).Seconds;
                    var totSeconds = Convert.ToDouble(prevDeviceStatus.MessageData[MessagePropertyName.TempIncreasingSec]) + DiffSeconds;
                    currentStatus.MessageData.Add(MessagePropertyName.TempIncreasingSec, Convert.ToString(totSeconds));

                }
                else
                {
                    currentStatus.MessageData.Add(MessagePropertyName.TempIncreasingSec, "0");
                }


                //increasing humidity
                if (Convert.ToDouble(currentStatus.MessageData[MessagePropertyName.Humidity]) > Convert.ToDouble(prevDeviceStatus.MessageData[MessagePropertyName.Humidity]))
                {
                    var DiffSeconds = (currentStatus.Timestamp - prevDeviceStatus.Timestamp).Seconds;
                    var totSeconds = Convert.ToDouble(prevDeviceStatus.MessageData[MessagePropertyName.HumIncreasingSec]) + DiffSeconds;
                    currentStatus.MessageData.Add(MessagePropertyName.HumIncreasingSec, Convert.ToString(totSeconds));

                }
                else
                {
                    currentStatus.MessageData.Add(MessagePropertyName.HumIncreasingSec, "0");
                }

            }

            //if we want to check alarm parameters in NewStateCalculator, make it here

            return currentStatus;
        }

        public static DeviceMessage UpdateTempOpenDoorType(DeviceMessage currentStatus)
        {
            //if there are no messages from that DeviceID, then create
            if (!DeviceTypesStatus[currentStatus.MessageType].ContainsKey(currentStatus.DeviceID))
            {
                currentStatus.MessageData.Add(MessagePropertyName.TempIncreasingSec, "0");
                currentStatus.MessageData.Add(MessagePropertyName.OpenDoorSec, "0");
            }
            else
            {
                var prevDeviceStatus = (DeviceTypesStatus[currentStatus.MessageType][currentStatus.DeviceID]).Last();

                //increasing temperature
                if (Convert.ToDouble(currentStatus.MessageData[MessagePropertyName.Temperature]) > Convert.ToDouble(prevDeviceStatus.MessageData[MessagePropertyName.Temperature]))
                {
                    var DiffSeconds = (currentStatus.Timestamp - prevDeviceStatus.Timestamp).Seconds;
                    var totSeconds = Convert.ToDouble(prevDeviceStatus.MessageData[MessagePropertyName.TempIncreasingSec]) + DiffSeconds;
                    currentStatus.MessageData.Add(MessagePropertyName.TempIncreasingSec, Convert.ToString(totSeconds));

                }
                else
                {
                    currentStatus.MessageData.Add(MessagePropertyName.TempIncreasingSec, "0");
                }


                //Door is still open
                if (Convert.ToBoolean(currentStatus.MessageData[MessagePropertyName.OpenDoor]) && Convert.ToBoolean(prevDeviceStatus.MessageData[MessagePropertyName.OpenDoor]))
                {
                    var DiffSeconds = (currentStatus.Timestamp - prevDeviceStatus.Timestamp).Seconds;
                    var totSeconds = Convert.ToDouble(prevDeviceStatus.MessageData[MessagePropertyName.OpenDoorSec]) + DiffSeconds;
                    currentStatus.MessageData.Add(MessagePropertyName.OpenDoorSec, Convert.ToString(totSeconds));

                }
                else
                {
                    currentStatus.MessageData.Add(MessagePropertyName.OpenDoorSec, "0");
                }
            }
            

            //if we want to check alarm parameters in NewStateCalculator, make it here

            return currentStatus;

        }

        public static void SendToQueue(string statusToSend, string queueName, string sbConnectionString, DeviceMessage status)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            
            writer.Write(statusToSend);
            writer.Flush();
            stream.Position = 0;
            
            var bm = new BrokeredMessage(stream)
            {
                Label = "NewStateForDevice " + status.DeviceID
            };
            bm.Properties["DeviceType"] = status.MessageType;

            var queue3Client = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queue3Client.Send(bm);
            
        }
        
    }
}
