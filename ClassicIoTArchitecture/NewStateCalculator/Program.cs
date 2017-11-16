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

namespace NewStateCalculator
{
    class Program
    {
        public static BatmanState prevBatmanState;
        public static JokerState prevJokerState;
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        
        static void Main(string[] args)
        {
            Console.WriteLine("NewStateCalculator - reads messages from T2, calculates new State for each device. Ctrl-C to exit.\n");
            var topicName = "sbtopic2";
            var subscriptionName = "toNewState";
            var client = SubscriptionClient.CreateFromConnectionString(sbConnectionString, topicName, subscriptionName);

            prevBatmanState = new BatmanState();
            prevJokerState = new JokerState();
            
            //readfromtopic  
            client.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();

                Console.WriteLine("received message from topic: {0}", s);


                JObject json = JObject.Parse(s);
                if (json["deviceId"].Value<string>() == "Batman")
                {
                    UpdateBatmanState(json);
                    var state = JsonConvert.SerializeObject(prevBatmanState);
                    SendToQueue(state, "Batman");
                    Console.WriteLine("message from Batman sent to Q3: {0}", state);
                }
                else if (json["deviceId"].Value<string>() == "Joker")
                {
                    UpdateJokerState(json);
                    var state = JsonConvert.SerializeObject(prevBatmanState);
                    SendToQueue(state, "Joker");
                    Console.WriteLine("message from Joker sent to Q3: {0}", state);

                }

            });
            Console.ReadLine();
            
        }

        public static void UpdateBatmanState(JObject json)
        {
            BatmanState currentBatmanState = new BatmanState
            {
                DeviceID = "Batman",
                Temperature = json["temperature"].Value<double>(),
                Humidity = json["humidity"].Value<double>()
            };

            if (currentBatmanState.Temperature >= prevBatmanState.Temperature)
            {
                currentBatmanState.CountTempIncreasing = prevBatmanState.CountTempIncreasing+1;
            }
            else
            {
                currentBatmanState.CountTempIncreasing = 0;
            }
            if (currentBatmanState.Humidity >= prevBatmanState.Humidity)
            {
                currentBatmanState.CountHumIncreasing = prevBatmanState.CountHumIncreasing+1;
            }
            else
            {
                currentBatmanState.CountHumIncreasing = 0;
            }
           
            prevBatmanState = currentBatmanState;
        }

        public static void UpdateJokerState(JObject json)
        {
            JokerState currentJokerState = new JokerState
            {
                DeviceID = "Joker",

                Temperature = json["temperature"].Value<double>(),
                IsOpen = json["temperature"].Value<bool>()
            };

            if (currentJokerState.Temperature >= prevJokerState.Temperature)
            {
                currentJokerState.TempIncreasingCount = prevJokerState.TempIncreasingCount+1;
            }
            else
            {
                currentJokerState.TempIncreasingCount = 0;
            }
            if (prevJokerState.IsOpen && currentJokerState.IsOpen)
            {
                currentJokerState.DoorOpenCount = prevJokerState.DoorOpenCount+1;
            }
            else
            {
                currentJokerState.DoorOpenCount = 0;
            }
           
            prevJokerState = currentJokerState;

        }

        public static void SendToQueue(string state, string deviceID)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            
            writer.Write(state);
            writer.Flush();
            stream.Position = 0;
            
            var bm = new BrokeredMessage(stream)
            {
                Label = "NewStateForDevice " + deviceID
            };
            bm.Properties["deviceId"] = deviceID;


            var queueName = "sbqueue3";
            var queue3Client = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queue3Client.Send(bm);
            
        }
        
    }
}
