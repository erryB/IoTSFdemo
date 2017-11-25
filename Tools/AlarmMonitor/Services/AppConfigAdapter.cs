using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlarmMonitor.Services
{
    public class AppConfigAdapter : IConfigurationAdapter
    {
        public Task<T> GetConfigurationValueAsync<T>(string configKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            T retVal = default(T);
            var configValue = ConfigurationManager.AppSettings[configKey];
            try
            {
                retVal = (T)Convert.ChangeType(configValue, typeof(T));
            }
            catch { }
            return Task.FromResult(retVal);
        }
    }
}
