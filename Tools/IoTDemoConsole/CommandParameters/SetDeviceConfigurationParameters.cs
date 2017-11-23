using IoTDemoConsole.Attributes;

namespace IoTDemoConsole.CommandParameters
{


    /// <summary>
    /// Class SetDeviceConfigurationParameters.
    /// </summary>
    /// <seealso cref="DeviceCommandParametersBase" />
    public class SetDeviceConfigurationParameters : DeviceCommandParametersBase
    {


        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [ParameterMetadata("fileName=", "nome completo del file contenete il JSON della configurazione.")]
        [ParameterRequired("Il parametro fileName è obbligatorio")]
        public virtual string FileName { get; set; }

    }
}