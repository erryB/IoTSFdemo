using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;

namespace SimulatedDeviceB
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iothubURI = "EBIoTHubDemo.azure-devices.net";
        static string deviceKey = "5rsBW8WFm/xkmWh8HZtxS2uhK5CFk7a0B3QY0ouwI08=";

        static string DeviceID = "Batman";

        static void Main(string[] args)
        {
            Console.WriteLine($"Simulated device: {DeviceID}\n");

            deviceClient = DeviceClient.Create(iothubURI, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceID, deviceKey));

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            int messageID = 1;
            Random rand = new Random();

            while (true)
            {

                string level = null;
                Object telemetryDataPoint = null;

                //random check for "critical" messages
                if (rand.NextDouble() > 0.9)
                {
                    //then send a "critical message"
                    telemetryDataPoint = new
                    {
                        MessageID = messageID++,
                        DeviceID = DeviceID,
                        Message = "WARNING - Low Battery"
                    };
                    level = "critical";
                }
                else
                {
                    double currentTemperature = Math.Round((minTemperature + rand.NextDouble() * 15), 2);
                    double currentHumidity = Math.Round((minHumidity + rand.NextDouble() * 20), 2);

                    telemetryDataPoint = new
                    {
                        MessageID = messageID++,
                        DeviceID = DeviceID,
                        Temperature = currentTemperature,
                        Humidity = currentHumidity,

                    };
                    level = "normal";
                }

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
