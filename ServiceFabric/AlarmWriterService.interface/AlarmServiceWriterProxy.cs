using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using CommonResources;

namespace AlarmWriterService.Interfaces
{
    public static class AlarmServiceWriterProxy
    {
        public static Task SendAlarmAsync(string deviceId, string alarmMessage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var hashDeviceId = deviceId.GetExtendedHash();
            var partitionKey = new ServicePartitionKey(hashDeviceId);
            var proxyAlarmWriter = ServiceProxy.Create<IAlarmWriterService>(new Uri("fabric:/EBIoTApplication/AlarmWriterService"), partitionKey);
            return proxyAlarmWriter.SendAlarmAsync(alarmMessage, cancellationToken);
        }
    }
}
