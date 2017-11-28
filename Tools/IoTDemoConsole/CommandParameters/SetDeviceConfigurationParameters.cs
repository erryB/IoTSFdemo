using System;
using IoTDemoConsole.Attributes;

namespace IoTDemoConsole.CommandParameters
{


    /// <summary>
    /// Class SetDeviceConfigurationParameters.
    /// </summary>
    public class SetDeviceConfigurationParameters
    {

        /// <summary>
        /// Gets or sets the service URI.
        /// </summary>
        /// <value>The service URI.</value>
        [ParameterMetadata("serviceUri=", "ServiceUri del device.")]
        public  string ServiceUri { get; set; }


        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        [ParameterMetadata("deviceId=", "Id del device.")]
        public  string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [ParameterMetadata("fileName=", "Nome completo del file contenete il JSON della configurazione. Se impostato questo parametro, i parametri dei singoli valori vengono ignorati")]
        public  string FileName { get; set; }

        [ParameterMetadata("tempThreshold=", "Soglia minima della temperatura. Il parametro viene preso in considerazione solo se non e' impostato il parametro fileName")]
        public  double? TemperatureThreshold { get; set; }

        [ParameterMetadata("humThreshold=", "Soglia minima dell'umidita'. Il parametro viene preso in considerazione solo se non e' impostato il parametro fileName")]
        public  double? HumidityThreshold { get; set; }

        [ParameterMetadata("doorDuration=", "Numero di secondi dopo dopo i quali scatta il segnale di allarme per porta aperta. Il parametro viene preso in considerazione solo se non e' impostato il parametro fileName")]
        public  int? OpenDoorDuration { get; set; }

        [ParameterMetadata("createFile", "Crea il file JSON per la configurazione", ParameterMetadataAttribute.ParameterType.Switch)]
        public  bool CreateFile { get; set; }


    }
}