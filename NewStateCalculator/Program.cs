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
                    updateBatmanState(json);
                    var state = JsonConvert.SerializeObject(prevBatmanState);
                    sendToQueue(state, "Batman");
                }
                else if (json["deviceId"].Value<string>() == "Joker")
                {
                    updateJokerState(json);
                    var state = JsonConvert.SerializeObject(prevBatmanState);
                    sendToQueue(state, "Joker");
                }

            });
            Console.ReadLine();




        }

        public static void updateBatmanState(JObject json)
        {
            BatmanState currentBatmanState = new BatmanState();
            currentBatmanState.Temperature = json["temperature"].Value<double>();
            currentBatmanState.Humidity = json["humidity"].Value<double>();
            if (prevBatmanState != null)
            {
                if (prevBatmanState.Temperature < currentBatmanState.Temperature)
                {
                    currentBatmanState.CountTempIncreasing = prevBatmanState.CountTempIncreasing++;
                }
                else
                {
                    currentBatmanState.CountTempIncreasing = 0;
                }
                if (prevBatmanState.Humidity < currentBatmanState.Humidity)
                {
                    currentBatmanState.CountTempIncreasing = prevBatmanState.CountTempIncreasing++;
                }
                else
                {
                    currentBatmanState.CountHumIncreasing = 0;
                }
            }
            prevBatmanState = currentBatmanState;
        }

        public static void updateJokerState(JObject json)
        {
            JokerState currentJokerState = new JokerState();
            currentJokerState.Temperature = json["temperature"].Value<double>();
            currentJokerState.IsOpen = json["temperature"].Value<bool>();
            if (prevJokerState != null)
            {
                if (prevJokerState.Temperature < currentJokerState.Temperature)
                {
                    currentJokerState.TempIncreasingCount = prevJokerState.TempIncreasingCount++;
                }
                else
                {
                    currentJokerState.TempIncreasingCount = 0;
                }
                if (prevJokerState.IsOpen && currentJokerState.IsOpen)
                {
                    currentJokerState.DoorOpenCount = prevJokerState.DoorOpenCount++;
                }
                else
                {
                    currentJokerState.DoorOpenCount = 0;
                }
            }
            prevJokerState = currentJokerState;

        }

        public static void sendToQueue(string state, string deviceID)
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
            
            var queueName = "sbqueue3";
            var queue3Client = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queue3Client.Send(bm);
            Console.WriteLine("message sent to Q3 -> {0}", deviceID);


        }

        
    }
}
