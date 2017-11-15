using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlarmWriterService.Interfaces
{
    public interface IAlarmWriterService : IService
    {
        Task SendAlarmAsync(string alarmMessage, CancellationToken cancelationToken);
    }
}
