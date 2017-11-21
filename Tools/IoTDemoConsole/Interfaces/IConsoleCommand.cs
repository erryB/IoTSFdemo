using System.Collections.Generic;

namespace IoTDemoConsole.Interfaces
{

    /// <summary>
    /// Interface IConsoleCommand
    /// </summary>
    public interface IConsoleCommand
    {

        /// <summary>
        /// Executes the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        void Execute(IList<string> arguments);


        /// <summary>
        /// Gets or sets the console.
        /// </summary>
        /// <value>The console.</value>
        IConsoleOutput Console { get; set; }
    }
}
