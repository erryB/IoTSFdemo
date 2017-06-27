using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedDeviceJ
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "EBIoTHubDemo.azure-devices.net";
        static string deviceKey = "two3s66XZ/GxuSWVSct5xUU1cLju9chicXKqOoG+uVk=";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device: Joker\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("Joker", deviceKey), TransportType.Mqtt);

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            //bool doorOpen = false;
            int messageId = 1;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = Math.Round((minTemperature + rand.NextDouble() * 15), 2);
                bool currentlyOpen = rand.NextDouble() > 0.5;
                string level = ((currentTemperature > 30) && currentlyOpen ? "critical" : "normal");

                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    deviceId = "Joker",
                    temperature = currentTemperature,
                    doorOpen = currentlyOpen
                    
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("level", level);

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message - level: {1} - {2}", DateTime.Now, level, messageString);

                await Task.Delay(1000);
            }
        }
    }
}
