using System;

namespace IoTDemoConsole.Attributes
{

    /// <summary>
    /// Class ParameterConstraintAttributeBase.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public abstract class ParameterConstraintAttributeBase : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterConstraintAttributeBase"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="System.ArgumentNullException">errorMessage</exception>
        protected ParameterConstraintAttributeBase(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentNullException(nameof(errorMessage));

            this.ErrorMessage = errorMessage;
        }


        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; protected set; }


        /// <summary>
        /// Returns true if the parameter passed by argument is valid.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if the specified parameter value is valid; otherwise, <c>false</c>.</returns>
        public abstract bool IsValid(object parameterValue, Type objectType);


        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string GetHelp()
        {
            return null;
        }
    }
}
