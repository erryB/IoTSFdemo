using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmServiceInterfaces
{
    public interface IAlarmService : IService
    {
        Task ReceiveAlarmAsync(string alarmMessage, string device);
    }
}
