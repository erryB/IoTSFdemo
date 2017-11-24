using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlarmMonitor.Services;

namespace AlarmMonitor.Models
{
    public class DeviceInfo : NotifyObject
    {
        public DeviceInfo()
        {
            DisplayTimeout = TimeSpan.FromMinutes(1);
        }

        public string DeviceId
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public AlarmMessage LastAlarmMessage
        {
            get { return GetPropertyValue<AlarmMessage>(); }
            set { SetPropertyValue(value); }
        }

        public TimeSpan DisplayTimeout
        {
            get { return GetPropertyValue<TimeSpan>(); }
            set { SetPropertyValue(value); }
        }

        public bool IsToBeRemoved()
        {
            var result = false;
            var lastAlarm = LastAlarmMessage;
            if (lastAlarm != null)
            {
                var timeout = DateTime.Now.Subtract(lastAlarm.TimeStamp);
                result = timeout > DisplayTimeout;
            }
            return result;
        }
    }
}
