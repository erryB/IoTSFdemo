using System;
using System.Linq;

namespace IoTDemoConsole.Attributes
{

    /// <summary>
    /// Class ParameterIntoSetAttribute.
    /// </summary>
    /// <seealso cref="IoTDemoConsole.Attributes.ParameterConstraintAttributeBase" />
    public class ParameterIntoSetAttribute : ParameterConstraintAttributeBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterIntoSetAttribute"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="valueSet">The value set.</param>
        /// <exception cref="System.ArgumentNullException">valueSet</exception>
        /// <exception cref="System.ArgumentException">valueSet</exception>
        public ParameterIntoSetAttribute(string errorMessage, params object[] valueSet) : base(errorMessage)
        {
            if (valueSet == null)
                throw new ArgumentNullException(nameof(valueSet));
            if (!valueSet.Any())
                throw new ArgumentException(nameof(valueSet));

            this.ValueSet = valueSet;
        }

        /// <summary>
        /// Gets the value set.
        /// </summary>
        /// <value>The value set.</value>
        public object[] ValueSet { get; private set; }


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
            try
            {
                return ValueSet.Any(o => o.Equals(parameterValue));
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
            if (ValueSet.Count()==1)
                return $"Valore ammesso: {ValueSet[0]}";
            return $"Valori ammessi: {ValueSet.Select(a => a.ToString()).Aggregate((a, b) => a + ", " + b)}";
        }
    }
}
