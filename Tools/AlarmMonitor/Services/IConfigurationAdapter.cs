using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlarmMonitor.Services
{
    public interface IConfigurationAdapter
    {
        Task<T> GetConfigurationValueAsync<T>(string configKey,CancellationToken cancellationToken = default(CancellationToken));
    }
}
