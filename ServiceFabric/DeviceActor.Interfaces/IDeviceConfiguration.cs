using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using CommonResources;

namespace DeviceActor.Interfaces
{

    /// <summary>
    /// Defines the methods to configure a device
    /// </summary>
    /// <seealso cref="Microsoft.ServiceFabric.Actors.IActor" />
    public interface IDeviceConfiguration : IActor
    {
        /// <summary>
        /// Sets the configuration asynchronous.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> SetConfigurationAsync(DeviceConfigurationData config, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the configuration asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;DeviceConfigurationData&gt;.</returns>
        Task<DeviceConfigurationData> GetConfigurationAsync(CancellationToken cancellationToken);
    }
}
