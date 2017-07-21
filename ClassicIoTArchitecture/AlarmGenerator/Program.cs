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

namespace AlarmGenerator 
{
    class Program
    {
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        

        static void Main(string[] args)
        {
            Console.WriteLine("AlarmGenerator - reads messages from Q3 and generates alarms. Ctrl-C to exit.\n");
            var queueName = "sbqueue3";
            var queueClient = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);

            queueClient.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();

                var msg = JsonConvert.DeserializeObject(s);
                JObject json = JObject.Parse(s);

                //Console.WriteLine("message read from Q3: {0}", s);

                try
                {
                    if (json["DeviceID"].Value<string>() == "Batman")
                    {
                        var hum = json["Humidity"].Value<double>();
                        var incT = json["CountTempIncreasing"].Value<int>();
                        if (hum > 50 && incT >= 2)
                        {//send alarm
                            string alarm = "ALARM from Batman - hum: " + hum + " increasing Temperature from " + incT + " cycles.";
                            Console.WriteLine(alarm);
                            
                        }
                    }
                    else if (json["DeviceID"].Value<string>() == "Joker")
                    {
                        var temp = json["Temperature"].Value<double>();
                        var openD = json["DoorOpen"].Value<int>();
                        if (temp > 25 && openD >= 2)
                        {
                            string alarm = "ALARM from Joker - temperature: " + temp + " and the door is open from " + openD + " cycles.";
                            Console.WriteLine(alarm);
                        }
                    }
                } catch (Exception e)
                {

                }
                
                


            });

            Console.ReadLine();
        }

        
    }
}
