using System;
using IoTDemoConsole.Attributes;

namespace IoTDemoConsole.CommandParameters
{
    public class GetDeviceConfigurationParameters
    {

        /// <summary>
        /// Gets or sets the service URI.
        /// </summary>
        /// <value>The service URI.</value>
        [ParameterMetadata("serviceUri=", "ServiceUri del device.")]
        [ParameterRequired("Il parametro serviceUri è obbligatorio")]
        public  string ServiceUri { get; set; }


        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        [ParameterMetadata("deviceId=", "Id del device.")]
        [ParameterRequired("Il parametro deviceId è obbligatorio")]
        public  string DeviceId { get; set; }


    }
}