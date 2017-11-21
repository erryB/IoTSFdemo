using System;

namespace IoTDemoConsole.Extensions
{

    /// <summary>
    /// Class StringExtension.
    /// </summary>
    public static class StringExtension
    {

        /// <summary>
        /// Adds the point to the end.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.NullReferenceException"></exception>
        public static string AddPointToTheEnd(this string value)
        {
            if (value == null)
                throw new NullReferenceException();

            if (value.TrimEnd(' ').EndsWith("."))
                return value;
            return string.Concat(value, ".");
        }
    }
}
