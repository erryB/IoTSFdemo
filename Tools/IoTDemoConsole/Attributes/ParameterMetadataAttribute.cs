using System;

namespace IoTDemoConsole.Attributes
{

    /// <summary>
    /// Class ParameterMetadataAttribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ParameterMetadataAttribute : Attribute
    {

        /// <summary>
        /// Enum ParameterType
        /// </summary>
        public enum ParameterType
        {

            /// <summary>
            /// The parameter
            /// </summary>
            Parameter,

            /// <summary>
            /// The switch
            /// </summary>
            Switch
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterMetadataAttribute"/> class.
        /// </summary>
        /// <param name="prototype">The prototype.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentNullException">
        /// prototype
        /// or
        /// description
        /// </exception>
        public ParameterMetadataAttribute(string prototype, string description, ParameterType type = ParameterType.Parameter)
        {
            if (string.IsNullOrWhiteSpace(prototype))
                throw new ArgumentNullException(nameof(prototype));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            this.Prototype = prototype;
            this.Description = description;
            this.Type = type;
        }


        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the prototype.
        /// </summary>
        /// <value>The prototype.</value>
        public string Prototype { get; private set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public ParameterType Type { get; private set; }
    }
}
