using System;
using System.IO;
using IoTDemoConsole.Interfaces;

namespace IoTDemoConsole.Outputs
{

    /// <summary>
    /// Class StandardConsoleOutput.
    /// </summary>
    /// <seealso cref="IoTDemoConsole.Interfaces.IConsoleOutput" />
    public class StandardConsoleOutput : IConsoleOutput
    {

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }


        /// <summary>
        /// Displays the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }


        /// <summary>
        /// Displays the result.
        /// </summary>
        /// <param name="message">The message.</param>
        public void DisplayResult(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }


        /// <summary>
        /// Gets the out.
        /// </summary>
        /// <value>The out.</value>
        public TextWriter Out
        {
            get { return Console.Out; }
        }


        /// <summary>
        /// Displays the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        public void DisplayWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }


    }
}
