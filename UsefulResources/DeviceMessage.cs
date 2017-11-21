﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UsefulResources
{
    [DataContract]
    public class DeviceMessage
    {
        [DataMember]
        public string DeviceID { get; set; }
        [DataMember]
        public int MessageID { get; set; }
        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public string MessageType { get; set; }
        [DataMember]
        public Dictionary<string, string> MessageData { get; set; }

        //create a DeviceMessage from a string that already contains MessageType and all the fields (previously formatted)
        public DeviceMessage(string messageString)
        {
            var msg = JsonConvert.DeserializeObject(messageString);
            JObject json = JObject.Parse(messageString);

            if (messageString.Contains(MessagePropertyName.MessageType))
            {
                this.DeviceID = json[MessagePropertyName.DeviceID].Value<string>();
                this.MessageID = Int32.Parse(json[MessagePropertyName.MessageID].Value<string>());
                this.Timestamp = DateTime.Parse(json[MessagePropertyName.Timestamp].Value<string>());
                this.MessageType = json[MessagePropertyName.MessageType].Value<string>();

                //how to assign to dictionary??
                if (this.MessageData == null)
                {
                    if (this.MessageType == MessagePropertyName.TempHumType)
                    {
                        this.MessageData = new Dictionary<string, string>
                        {
                            { MessagePropertyName.Temperature, json[MessagePropertyName.Temperature].Value<double>().ToString() },
                            { MessagePropertyName.Humidity, json[MessagePropertyName.Humidity].Value<double>().ToString() }
                        };

                    }
                    else if (this.MessageType == MessagePropertyName.TempOpenDoorType)
                    {
                        this.MessageData = new Dictionary<string, string>
                        {
                            { MessagePropertyName.Temperature, json[MessagePropertyName.Temperature].Value<double>().ToString() },
                            { MessagePropertyName.OpenDoor, json[MessagePropertyName.OpenDoor].Value<double>().ToString() }
                        };
                    } else
                    {
                        this.MessageData = new Dictionary<string, string>();
                    }
                }


            }
            else
            {
                new DeviceMessage(messageString, DateTime.Now);
            }
        }

        //create a DeviceMessage from a string with no MessageType 
        public DeviceMessage(string messageString, DateTime timestamp)
        {

            this.MessageData = new Dictionary<string, string>();

            var msg = JsonConvert.DeserializeObject(messageString);
            JObject json = JObject.Parse(messageString);
            try
            {
                this.DeviceID = json[MessagePropertyName.DeviceID].Value<string>();
                this.MessageID = json[MessagePropertyName.MessageID].Value<int>();
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
                return $"unknown message type from: {DeviceID}";
            }

            return result;


        }




    }
}