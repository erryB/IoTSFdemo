using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonResources
{
    [DataContract]
    public class DeviceMessage
    {
        [DataMember]
        public string DeviceID { get; set; }

        [DataMember]
        public Guid MessageID { get; set; } = Guid.NewGuid();
        [DataMember]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        [DataMember]
        public string MessageType { get; set; }
        [DataMember]
        public Dictionary<string, string> MessageData { get; set; }

        public DeviceMessage()
        {
            this.MessageData = new Dictionary<string, string>();
        }

        public DeviceMessage(string messageString, DateTime timestamp)
        {
            this.MessageData = new Dictionary<string, string>();

            var msg = JsonConvert.DeserializeObject(messageString);
            JObject json = JObject.Parse(messageString);
            try
            {
                this.DeviceID = json[MessagePropertyName.DeviceID].Value<string>();
                this.MessageID = Guid.Parse(json[MessagePropertyName.MessageID].Value<string>());
                this.Timestamp = timestamp;
                //MessageType is different for Batman and Joker devices 
                if (messageString.Contains(MessagePropertyName.Temperature) && messageString.Contains(MessagePropertyName.Humidity))
                {
                    this.MessageType = MessagePropertyName.TempHumType;
                    this.MessageData.Add(MessagePropertyName.Temperature, json[MessagePropertyName.Temperature].Value<double>().ToString());
                    this.MessageData.Add(MessagePropertyName.Humidity, json[MessagePropertyName.Humidity].Value<double>().ToString());

                }
                else if (messageString.Contains(MessagePropertyName.Temperature) && messageString.Contains(MessagePropertyName.OpenDoor))
                {
                    this.MessageType = MessagePropertyName.TempOpenDoorType;
                    this.MessageData.Add(MessagePropertyName.Temperature, json[MessagePropertyName.Temperature].Value<double>().ToString());
                    this.MessageData.Add(MessagePropertyName.OpenDoor, json[MessagePropertyName.OpenDoor].Value<bool>().ToString());
                }
                else
                {
                    this.MessageType = MessagePropertyName.UnknownType;
                }

            }
            catch (Exception)
            {
                this.DeviceID = "MESSAGE ERROR";
                this.MessageType = MessagePropertyName.UnknownType;
            }

        }

        public override string ToString()
        {
            string result = $"Message received from {DeviceID}, Timestamp {Timestamp}, Message Type: {MessageType}, Message ID: {MessageID}";

            if (this.MessageType == MessagePropertyName.TempHumType)
            {
                result += $" Temperature: {MessageData[MessagePropertyName.Temperature]}, Humidity: {MessageData[MessagePropertyName.Humidity]}";
            }
            else if (MessageType == MessagePropertyName.TempOpenDoorType)
            {
                result += $" Temperature: {MessageData[MessagePropertyName.Temperature]}, Open Door: {MessageData[MessagePropertyName.OpenDoor]}";
            }
            else //unknown message type   
            {
                return DeviceID;
            }

            return result;


        }


    }
}
