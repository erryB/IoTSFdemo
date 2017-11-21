using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulResources;

namespace NewStateCalculator
{
    class Program
    {
        
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        public static string topicName = "sbtopic2";
        public static string subscriptionName = "toNewState";
        public static string queueName = "sbqueue3";

        
        //key: DeviceType
        public static Dictionary<string, Dictionary<string, List<DeviceStatus>>> DeviceTypesStatus;
       

        static void Main(string[] args)
        {
            Console.WriteLine($"NewStateCalculator - reads messages from {topicName}, calculates new Status for each device. Ctrl-C to exit.\n");
            
            var client = SubscriptionClient.CreateFromConnectionString(sbConnectionString, topicName, subscriptionName);

            //init dictionary. If you have more DeviceTypes you can use a List
            DeviceTypesStatus = new Dictionary<string, Dictionary<string, List<DeviceStatus>>>
            {
                {
                    StatusPropertyName.TempHumType,
                    new Dictionary<string, List<DeviceStatus>>()
                },
                {
                    StatusPropertyName.TempOpenDoorType,
                    new Dictionary<string, List<DeviceStatus>>()
                },
                {
                    StatusPropertyName.UnknownType,
                    new Dictionary<string, List<DeviceStatus>>()
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

                //create the related DeviceStatus
                DeviceStatus currentDeviceStatus = new DeviceStatus
                {
                    DeviceID = receivedMessage.DeviceID,
                    DeviceType = receivedMessage.MessageType,
                    Timestamp = receivedMessage.Timestamp,
                    Data = receivedMessage.MessageData,
                    MessageID = receivedMessage.MessageID
                };

                DeviceStatus updatedStatus;

                //status update based on the DeviceType
                if (currentDeviceStatus.DeviceType == StatusPropertyName.TempHumType)
                {
                    updatedStatus = UpdateTempHumType(currentDeviceStatus);
                }
                else if (currentDeviceStatus.DeviceType == StatusPropertyName.TempOpenDoorType)
                {
                    updatedStatus = UpdateTempOpenDoorType(currentDeviceStatus);
                }
                else
                {
                    updatedStatus = new DeviceStatus
                    {
                        DeviceID = StatusPropertyName.UnknownType,
                        DeviceType = StatusPropertyName.UnknownType
                       
                    };
                }

                //send to Q
                var StatusToSend = JsonConvert.SerializeObject(updatedStatus);
                Console.WriteLine($"STATUS TO SEND: {StatusToSend}");
                SendToQueue(StatusToSend, queueName, sbConnectionString, updatedStatus);
                Console.WriteLine($"STATUS SENT to {queueName}: {StatusToSend}");

                //update dictionary

                //if the currentDeviceID does not exist in the dictionary, then add it. Otherwise update with a new item on its list
                if (!DeviceTypesStatus[updatedStatus.DeviceType].ContainsKey(updatedStatus.DeviceID))
                {
                    DeviceTypesStatus[updatedStatus.DeviceType].Add(updatedStatus.DeviceID, new List<DeviceStatus> { updatedStatus });
                }
                else
                {
                    DeviceTypesStatus[updatedStatus.DeviceType][updatedStatus.DeviceID].Add(updatedStatus);
                }
                

                //JObject json = JObject.Parse(s);
                //if (json["deviceId"].Value<string>() == "Batman")
                //{
                //    UpdateBatmanState(json);
                //    var state = JsonConvert.SerializeObject(prevBatmanState);
                //    SendToQueue(state, "Batman");
                //    Console.WriteLine("message from Batman sent to Q3: {0}", state);
                //}
                //else if (json["deviceId"].Value<string>() == "Joker")
                //{
                //    UpdateJokerState(json);
                //    var state = JsonConvert.SerializeObject(prevBatmanState);
                //    SendToQueue(state, "Joker");
                //    Console.WriteLine("message from Joker sent to Q3: {0}", state);

                //}

            });
            Console.ReadLine();
            
        }

        public static DeviceStatus UpdateTempHumType(DeviceStatus currentStatus)
        {
            //if there are no messages from that DeviceID, then create
            if (!DeviceTypesStatus[currentStatus.DeviceType].ContainsKey(currentStatus.DeviceID))
            {
                currentStatus.Data.Add(StatusPropertyName.TempIncreasingSec, "0");
                currentStatus.Data.Add(StatusPropertyName.HumIncreasingSec, "0");
            } else
            {
                var prevDeviceStatus = (DeviceTypesStatus[currentStatus.DeviceType][currentStatus.DeviceID]).Last();

                //increasing temperature
                if (Convert.ToDouble(currentStatus.Data[StatusPropertyName.Temperature]) > Convert.ToDouble(prevDeviceStatus.Data[StatusPropertyName.Temperature]))
                {
                    var DiffSeconds = (currentStatus.Timestamp - prevDeviceStatus.Timestamp).Seconds;
                    var totSeconds = Convert.ToDouble(prevDeviceStatus.Data[StatusPropertyName.TempIncreasingSec]) + DiffSeconds;
                    currentStatus.Data.Add(StatusPropertyName.TempIncreasingSec, Convert.ToString(totSeconds));

                }
                else
                {
                    currentStatus.Data.Add(StatusPropertyName.TempIncreasingSec, "0");
                }


                //increasing humidity
                if (Convert.ToDouble(currentStatus.Data[StatusPropertyName.Humidity]) > Convert.ToDouble(prevDeviceStatus.Data[StatusPropertyName.Humidity]))
                {
                    var DiffSeconds = (currentStatus.Timestamp - prevDeviceStatus.Timestamp).Seconds;
                    var totSeconds = Convert.ToDouble(prevDeviceStatus.Data[StatusPropertyName.HumIncreasingSec]) + DiffSeconds;
                    currentStatus.Data.Add(StatusPropertyName.HumIncreasingSec, Convert.ToString(totSeconds));

                }
                else
                {
                    currentStatus.Data.Add(StatusPropertyName.HumIncreasingSec, "0");
                }

            }

            //if we want to check alarm parameters in NewStateCalculator, make it here

            return currentStatus;
        }

        public static DeviceStatus UpdateTempOpenDoorType(DeviceStatus currentStatus)
        {
            //if there are no messages from that DeviceID, then create
            if (!DeviceTypesStatus[currentStatus.DeviceType].ContainsKey(currentStatus.DeviceID))
            {
                currentStatus.Data.Add(StatusPropertyName.TempIncreasingSec, "0");
                currentStatus.Data.Add(StatusPropertyName.OpenDoorSec, "0");
            }
            else
            {
                var prevDeviceStatus = (DeviceTypesStatus[currentStatus.DeviceType][currentStatus.DeviceID]).Last();

                //increasing temperature
                if (Convert.ToDouble(currentStatus.Data[StatusPropertyName.Temperature]) > Convert.ToDouble(prevDeviceStatus.Data[StatusPropertyName.Temperature]))
                {
                    var DiffSeconds = (currentStatus.Timestamp - prevDeviceStatus.Timestamp).Seconds;
                    var totSeconds = Convert.ToDouble(prevDeviceStatus.Data[StatusPropertyName.TempIncreasingSec]) + DiffSeconds;
                    currentStatus.Data.Add(StatusPropertyName.TempIncreasingSec, Convert.ToString(totSeconds));

                }
                else
                {
                    currentStatus.Data.Add(StatusPropertyName.TempIncreasingSec, "0");
                }


                //Door is still open
                if (Convert.ToBoolean(currentStatus.Data[StatusPropertyName.OpenDoor]) && Convert.ToBoolean(prevDeviceStatus.Data[StatusPropertyName.OpenDoor]))
                {
                    var DiffSeconds = (currentStatus.Timestamp - prevDeviceStatus.Timestamp).Seconds;
                    var totSeconds = Convert.ToDouble(prevDeviceStatus.Data[StatusPropertyName.OpenDoorSec]) + DiffSeconds;
                    currentStatus.Data.Add(StatusPropertyName.OpenDoorSec, Convert.ToString(totSeconds));

                }
                else
                {
                    currentStatus.Data.Add(StatusPropertyName.OpenDoorSec, "0");
                }
            }
            

            //if we want to check alarm parameters in NewStateCalculator, make it here

            return currentStatus;

        }

        //public static void UpdateBatmanState(JObject json)
        //{
        //    BatmanState currentBatmanState = new BatmanState
        //    {
        //        DeviceID = "Batman",
        //        Temperature = json["temperature"].Value<double>(),
        //        Humidity = json["humidity"].Value<double>()
        //    };

        //    if (currentBatmanState.Temperature >= prevBatmanState.Temperature)
        //    {
        //        currentBatmanState.CountTempIncreasing = prevBatmanState.CountTempIncreasing+1;
        //    }
        //    else
        //    {
        //        currentBatmanState.CountTempIncreasing = 0;
        //    }
        //    if (currentBatmanState.Humidity >= prevBatmanState.Humidity)
        //    {
        //        currentBatmanState.CountHumIncreasing = prevBatmanState.CountHumIncreasing+1;
        //    }
        //    else
        //    {
        //        currentBatmanState.CountHumIncreasing = 0;
        //    }
           
        //    prevBatmanState = currentBatmanState;
        //}

        //public static void UpdateJokerState(JObject json)
        //{
        //    JokerState currentJokerState = new JokerState
        //    {
        //        DeviceID = "Joker",

        //        Temperature = json["temperature"].Value<double>(),
        //        IsOpen = json["temperature"].Value<bool>()
        //    };

        //    if (currentJokerState.Temperature >= prevJokerState.Temperature)
        //    {
        //        currentJokerState.TempIncreasingCount = prevJokerState.TempIncreasingCount+1;
        //    }
        //    else
        //    {
        //        currentJokerState.TempIncreasingCount = 0;
        //    }
        //    if (prevJokerState.IsOpen && currentJokerState.IsOpen)
        //    {
        //        currentJokerState.DoorOpenCount = prevJokerState.DoorOpenCount+1;
        //    }
        //    else
        //    {
        //        currentJokerState.DoorOpenCount = 0;
        //    }
           
        //    prevJokerState = currentJokerState;

        //}

        public static void SendToQueue(string statusToSend, string queueName, string sbConnectionString, DeviceStatus status)
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
            bm.Properties["DeviceType"] = status.DeviceType;

            var queue3Client = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queue3Client.Send(bm);
            
        }
        
    }
}
