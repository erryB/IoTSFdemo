using IoTDemoConsole.Attributes;

namespace IoTDemoConsole.CommandParameters
{

    /// <summary>
    /// Class ActorParametersBase.
    /// </summary>
    public abstract class ActorParametersBase
    {

        /// <summary>
        /// Gets or sets the service URI.
        /// </summary>
        /// <value>The service URI.</value>
        [ParameterMetadata("serviceUri=", "ServiceUri dell'attore su cui eseguire la diagnostica")]
        [ParameterRequired("Il parametro serviceUri è obbligatorio")]
        public virtual string ServiceUri { get; set; }


        /// <summary>
        /// Gets or sets the actor identifier.
        /// </summary>
        /// <value>The actor identifier.</value>
        [ParameterMetadata("actorId=", "Id dell'attore su cui eseguire la diagnostica. Se impostato questo parametro, il parametro partitionId viene trascurato.")]
        public virtual string ActorId { get; set; }
    }
}