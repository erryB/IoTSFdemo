using System;
using IoTDemoConsole.Extensions;

namespace IoTDemoConsole.Attributes
{

    /// <summary>
    /// Class ParameterRangeAttribute.
    /// </summary>
    /// <seealso cref="IoTDemoConsole.Attributes.ParameterConstraintAttributeBase" />
    public class ParameterRangeAttribute : ParameterConstraintAttributeBase
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRangeAttribute"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="fromValue">From value.</param>
        /// <param name="toValue">To value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// fromValue
        /// or
        /// toValue
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// fromValue
        /// or
        /// toValue
        /// </exception>
        public ParameterRangeAttribute(string errorMessage, object fromValue, object toValue) : base(errorMessage)
        {
            if (fromValue == null)
                throw new ArgumentNullException(nameof(fromValue));
            if (!InputTypeIsValid(fromValue.GetType()))
                throw new ArgumentException($"{nameof(fromValue)} non è un tipo valido");
            if (toValue == null)
                throw new ArgumentNullException(nameof(toValue));
            if (!InputTypeIsValid(toValue.GetType()))
                throw new ArgumentException($"{nameof(toValue)} non è un tipo valido");

            this.FromValue = fromValue;
            this.ToValue = toValue;
        }

        /// <summary>
        /// Inputs the type is valid.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool InputTypeIsValid(Type type)
        {
            var result = true;
            result |= type.IsNumericType();
            result |= type == typeof(DateTime);
            return result;
        }


        /// <summary>
        /// Gets from value.
        /// </summary>
        /// <value>From value.</value>
        public object FromValue { get; private set; }


        /// <summary>
        /// Gets to value.
        /// </summary>
        /// <value>To value.</value>
        public object ToValue { get; private set; }

        /// <summary>
        /// Returns true if the parameter passed by argument is valid.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if the specified parameter value is valid; otherwise, <c>false</c>.</returns>
        public override bool IsValid(object parameterValue, Type objectType)
        {
            if (parameterValue == null)
                return false;
            if (!objectType.IsValueType)
                return false;
            try
            {
                return parameterValue.GreaterThan(FromValue) && parameterValue.LessThan(ToValue);
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return $"Valore compreso tra {FromValue} e {ToValue}";
        }
    }
}
