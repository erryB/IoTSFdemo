using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IoTDemoConsole.Attributes;
using IoTDemoConsole.Extensions;
using NDesk.Options;

namespace IoTDemoConsole.Helpers
{

    /// <summary>
    /// Class CommandParameterHelper.
    /// </summary>
    public static class CommandParameterHelper
    {
        /// <summary>
        /// Creates the option set from arguments.
        /// </summary>
        /// <typeparam name="TArguments">The type of the t arguments.</typeparam>
        /// <param name="arguments">The arguments.</param>
        /// <returns>OptionSet.</returns>
        public static OptionSet CreateOptionSetFromArguments<TArguments>(TArguments arguments)
            where TArguments : class, new()
        {
            var argumentType = arguments.GetType();
            var properties = argumentType.GetProperties()
                                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ParameterMetadataAttribute)));

            var optionSet = new OptionSet();
            foreach (var propertyInfo in properties)
            {
                CreateOptionFromPropertyInfo(arguments, optionSet, propertyInfo);
            }

            return optionSet;
        }

        /// <summary>
        /// Creates the option from property information.
        /// </summary>
        /// <typeparam name="TArguments">The type of the t arguments.</typeparam>
        /// <param name="arguments">The arguments.</param>
        /// <param name="optionSet">The option set.</param>
        /// <param name="propertyInfo">The property information.</param>
        private static void CreateOptionFromPropertyInfo<TArguments>(TArguments arguments,
            OptionSet optionSet, PropertyInfo propertyInfo)
            where TArguments : class, new()
        {
            var metadataAttribute = propertyInfo.GetCustomAttribute(typeof(ParameterMetadataAttribute)) as ParameterMetadataAttribute;
            if (metadataAttribute != null)
            {
                try
                {
                    Action<string> assignLambda = null;
                    switch (metadataAttribute.Type)
                    {
                        case ParameterMetadataAttribute.ParameterType.Switch:
                            assignLambda = s =>
                            {
                                if (propertyInfo.PropertyType == typeof(bool))
                                    propertyInfo.SetValue(arguments, true);
                            };
                            break;
                        case ParameterMetadataAttribute.ParameterType.Parameter:
                            assignLambda = s =>
                            {
                                object value;
                                if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null)
                                {
                                    value = Convert.ChangeType(s, Nullable.GetUnderlyingType(propertyInfo.PropertyType));
                                }
                                else
                                {
                                    value = Convert.ChangeType(s, propertyInfo.PropertyType);
                                }
                                propertyInfo.SetValue(arguments, value);
                            };
                            break;
                    }

                    var commandDescription = metadataAttribute.Description.AddPointToTheEnd();
                    var constraintHelp = GetConstraintHelpFromProperty(propertyInfo);
                    if (!string.IsNullOrWhiteSpace(constraintHelp))
                        commandDescription = string.Concat(commandDescription, " ", constraintHelp);
                    optionSet.Add(metadataAttribute.Prototype, commandDescription, assignLambda);
                }
                catch { }
            }
        }

        /// <summary>
        /// Gets the constraint help from property.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>System.String.</returns>
        private static string GetConstraintHelpFromProperty(PropertyInfo propertyInfo)
        {
            StringBuilder retValue = new StringBuilder();
            var metadataAttributes = propertyInfo.GetCustomAttributes(typeof(ParameterConstraintAttributeBase));
            if (metadataAttributes != null)
            {
                foreach (var metadataAttribute in metadataAttributes)
                {
                    try
                    {
                        var constraintHelp = ((ParameterConstraintAttributeBase)metadataAttribute).GetHelp();
                        if (!string.IsNullOrWhiteSpace(constraintHelp))
                        {
                            if (retValue.Length > 0)
                                retValue.Append(" ");
                            retValue.Append(constraintHelp.AddPointToTheEnd());
                        }
                    }
                    catch { }
                }
            }
            return retValue.ToString();
        }

        /// <summary>
        /// Checks the parameters.
        /// </summary>
        /// <typeparam name="TArguments">The type of the t arguments.</typeparam>
        /// <param name="arguments">The arguments.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public static IEnumerable<string> CheckParameters<TArguments>(TArguments arguments)
            where TArguments : class, new()
        {
            var argumentType = arguments.GetType();
            var properties = argumentType.GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType.IsSubclassOf(typeof(ParameterConstraintAttributeBase))))
                .ToList();

            List<string> result = new List<string>();
            foreach (var propertyInfo in properties)
            {
                var errors = CheckParameter(arguments, propertyInfo);
                if (errors != null && errors.Any())
                    result.AddRange(errors);
            }

            return result;
        }

        /// <summary>
        /// Checks the parameter.
        /// </summary>
        /// <typeparam name="TArguments">The type of the t arguments.</typeparam>
        /// <param name="arguments">The arguments.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        private static IEnumerable<string> CheckParameter<TArguments>(TArguments arguments, PropertyInfo propertyInfo)
            where TArguments : class, new()
        {
            List<string> errors = new List<string>();
            var constraintAttributes = propertyInfo.GetCustomAttributes(typeof(ParameterConstraintAttributeBase));
            if (constraintAttributes != null)
            {
                foreach (var constraintAttribute in constraintAttributes)
                {
                    try
                    {
                        if (!((ParameterConstraintAttributeBase)constraintAttribute).IsValid(propertyInfo.GetValue(arguments), propertyInfo.PropertyType))
                        {
                            errors.Add(((ParameterConstraintAttributeBase)constraintAttribute).ErrorMessage);
                        }
                    }
                    catch { }
                }

            }
            return errors;
        }
    }
}
