using System;
using IoTDemoConsole.Attributes;

namespace IoTDemoConsole.CommandParameters
{

    public class SendDeviceMessageParameters
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
        [ParameterRequired("Il parametro deviceId è obbligatorio")]
        public  string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [ParameterMetadata("fileName=", "Nome completo del file contenete il JSON della configurazione. Se impostato questo parametro, i parametri dei singoli valori vengono ignorati")]
        public virtual string FileName { get; set; }

        [ParameterMetadata("temperature=", "Temperatura corrente. Il parametro viene preso in considerazione solo se non e' impostato il parametro fileName")]
        public virtual double? Temperature { get; set; }

        [ParameterMetadata("humidity=", "Umidita' corrente. Il parametro viene preso in considerazione solo se non e' impostato il parametro fileName")]
        public virtual double? Humidity { get; set; }

        [ParameterMetadata("doorOpen=", "Porta aperta o chiusa. Il parametro viene preso in considerazione solo se non e' impostato il parametro fileName")]
        public virtual bool? DoorOpen { get; set; }

        [ParameterMetadata("createFile", "Crea il file JSON per la configurazione", ParameterMetadataAttribute.ParameterType.Switch)]
        public virtual bool CreateFile { get; set; }

    }
}