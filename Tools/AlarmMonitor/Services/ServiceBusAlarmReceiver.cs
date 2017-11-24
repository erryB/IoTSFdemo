using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmMonitor.Services
{
    public class ServiceBusAlarmReceiver : IAlarmReceiver
    {
        public Task StartReceiverAsync()
        {
            //throw new NotImplementedException();
            return Task.Delay(0);
        }

        public Task StopReceiverAsync()
        {
            //throw new NotImplementedException();
            return Task.Delay(0);
        }

        public event EventHandler<AlarmMessage> AlarmMessageReceived;
    }
}
