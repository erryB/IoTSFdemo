using System;
using IoTDemoConsole.Extensions;

namespace IoTDemoConsole.Attributes
{

    /// <summary>
    /// Class ParameterRequiredAttribute.
    /// </summary>
    /// <seealso cref="IoTDemoConsole.Attributes.ParameterConstraintAttributeBase" />
    public class ParameterRequiredAttribute : ParameterConstraintAttributeBase
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRequiredAttribute"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public ParameterRequiredAttribute(string errorMessage) : base(errorMessage)
        {
        }


        /// <summary>
        /// Returns true if the parameter passed by argument is valid.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if the specified parameter value is valid; otherwise, <c>false</c>.</returns>
        public override bool IsValid(object parameterValue, Type objectType)
        {
            var @default = objectType.GetDefault();
            if (objectType.IsValueType && Nullable.GetUnderlyingType(objectType) == null)
                return !parameterValue.Equals(@default);

            return parameterValue != @default;
        }


        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "Il parametro è obbligatorio.";
        }
    }
}
