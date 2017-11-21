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

namespace AlarmGenerator 
{
    class Program
    {
        public static string sbConnectionString = "Endpoint=sb://ebsbnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59Oq2KM+b5DNqRsoQ+qbua5Z7zG/7I/ohAHukC9eaKA=";
        public static string queueName = "sbqueue3";

        static void Main(string[] args)
        {
            Console.WriteLine($"AlarmGenerator - reads messages from {queueName} and generates alarms. Ctrl-C to exit.\n");
             
            var queueClient = QueueClient.CreateFromConnectionString(sbConnectionString, queueName);
            string alarm = null;

            queueClient.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();

                DeviceMessage currentMessage = new DeviceMessage(s);

                if(currentMessage.MessageType == MessagePropertyName.TempHumType)
                {
                    if(Convert.ToDouble(currentMessage.MessageData[MessagePropertyName.TempHumType]) > AlarmParameters.TemperatureTH && Convert.ToDouble(currentMessage.MessageData[MessagePropertyName.TempIncreasingSec]) > AlarmParameters.IncTempSecTH)
                    {
                        alarm = $"ALARM - Temperature is too high and keeps increasing - {s}";
                    }
                    
                } else if(currentMessage.MessageType == MessagePropertyName.TempOpenDoorType)
                {
                    if (Convert.ToDouble(currentMessage.MessageData[MessagePropertyName.TempHumType]) > AlarmParameters.TemperatureTH && Convert.ToDouble(currentMessage.MessageData[MessagePropertyName.OpenDoorSec]) > AlarmParameters.OpenDoorTH)
                    {
                        alarm = $"ALARM - Temperature is too high and the door is open. CLOSE THE DOOR - {s}";
                    }
                } else
                {
                    alarm = "ERROR";
                }

                Console.WriteLine(alarm);



                //var msg = JsonConvert.DeserializeObject(s);
                //JObject json = JObject.Parse(s);

                //Console.WriteLine("message read from Q3: {0}", s);

                //try
                //{
                //    if (json["DeviceID"].Value<string>() == "Batman")
                //    {
                //        var hum = json["Humidity"].Value<double>();
                //        var incT = json["CountTempIncreasing"].Value<int>();
                //        if (hum > 50 && incT >= 2)
                //        {//send alarm
                //            string alarm = "ALARM from Batman - hum: " + hum + " increasing Temperature from " + incT + " cycles.";
                //            Console.WriteLine(alarm);
                            
                //        }
                //    }
                //    else if (json["DeviceID"].Value<string>() == "Joker")
                //    {
                //        var temp = json["Temperature"].Value<double>();
                //        var openD = json["DoorOpen"].Value<int>();
                //        if (temp > 25 && openD >= 2)
                //        {
                //            string alarm = "ALARM from Joker - temperature: " + temp + " and the door is open from " + openD + " cycles.";
                //            Console.WriteLine(alarm);
                //        }
                //    }
                //} catch (Exception e)
                //{

                //}
                
                


            });

            Console.ReadLine();
        }

        
    }
}
