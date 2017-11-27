using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using System.Configuration;

namespace SimulatedDeviceB
{
    class Program
    {
        static DeviceClient deviceClient;
        //static string iothubURI = "EBIoTHubDemo.azure-devices.net";
        //static string deviceKey = "5rsBW8WFm/xkmWh8HZtxS2uhK5CFk7a0B3QY0ouwI08=";

        static string DeviceID = "Batman";

        static DateTime StartTime;
        static double minTemperature = 20;
        static double maxTemperature = 40;
        static int temperaturePeriodInSec = 120;
        static double minHumidity = 40;
        static double maxHumidity = 80;
        static int humidityPeriodInSec = 180;

        static void Main(string[] args)
        {
            StartTime = DateTime.Now;

            Console.WriteLine($"Simulated device: {DeviceID}\n");

            deviceClient = DeviceClient.Create(ConfigurationManager.AppSettings["iothubURI"], new DeviceAuthenticationWithRegistrySymmetricKey(DeviceID, ConfigurationManager.AppSettings["deviceKey"]));

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
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
                        MessageID = Guid.NewGuid(),
                        DeviceID = DeviceID,
                        Message = "WARNING - Low Battery"
                    };
                    level = "critical";
                }
                else
                {
                    double currentTemperature = Math.Round(CalculateSinValue(minTemperature, maxTemperature, temperaturePeriodInSec), 2);
                    double currentHumidity = Math.Round(CalculateSinValue(minHumidity, maxHumidity, humidityPeriodInSec), 2);

                    telemetryDataPoint = new
                    {
                        MessageID = Guid.NewGuid(),
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

        static double CalculateSinValue(double min, double max, int periodInSeconds)
        {
            var delta = (max - min) / 2;
            var t = DateTime.Now.Subtract(StartTime).TotalSeconds;
            return min + (Math.Sin(2 * Math.PI * t / periodInSeconds) + 1) * delta;
        }
    }
}
