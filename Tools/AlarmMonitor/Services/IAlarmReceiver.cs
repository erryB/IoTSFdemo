using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AlarmMonitor.Services
{
    public interface IAlarmReceiver
    {
        Task StartReceiverAsync();

        event EventHandler<AlarmMessage> AlarmMessageReceived;

        Task StopReceiverAsync();
    }

    public class AlarmMessage
    {
        [JsonProperty(PropertyName ="DeviceId")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "MesageId")]
        public string MessageId { get; set; }

        [JsonProperty(PropertyName = "AlarmMessage")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "TimeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}