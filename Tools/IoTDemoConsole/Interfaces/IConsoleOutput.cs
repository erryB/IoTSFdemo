using System.IO;

namespace IoTDemoConsole.Interfaces
{

    /// <summary>
    /// Interface IConsoleOutput
    /// </summary>
    public interface IConsoleOutput
    {

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        void DisplayError(string message);

        /// <summary>
        /// Displays the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void DisplayMessage(string message);

        /// <summary>
        /// Displays the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        void DisplayWarning(string message);

        /// <summary>
        /// Displays the result.
        /// </summary>
        /// <param name="message">The message.</param>
        void DisplayResult(string message);

        /// <summary>
        /// Gets the out.
        /// </summary>
        /// <value>The out.</value>
        TextWriter Out { get; }
    }
}