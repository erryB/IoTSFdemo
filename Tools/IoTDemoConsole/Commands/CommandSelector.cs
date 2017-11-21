using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IoTDemoConsole.Interfaces;

namespace IoTDemoConsole.Commands
{

    /// <summary>
    /// Class CommandSelector.
    /// </summary>
    public class CommandSelector
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandSelector"/> class.
        /// </summary>
        /// <param name="outputConsole">The output console.</param>
        /// <exception cref="System.ArgumentNullException">outputConsole</exception>
        public CommandSelector(IConsoleOutput outputConsole)
        {
            if (outputConsole == null)
                throw new ArgumentNullException(nameof(outputConsole));

            this.Console = outputConsole;
        }


        /// <summary>
        /// Gets the console.
        /// </summary>
        /// <value>The console.</value>
        public IConsoleOutput Console { get; private set; }


        /// <summary>
        /// The registered commands
        /// </summary>
        private readonly Dictionary<string, IConsoleCommand> _registeredCommands = new Dictionary<string, IConsoleCommand>();


        /// <summary>
        /// Sets the console description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns>CommandSelector.</returns>
        public CommandSelector SetConsoleDescription(string description)
        {
            this.ConsoleDescription = description;
            return this;
        }


        /// <summary>
        /// Gets or sets the console description.
        /// </summary>
        /// <value>The console description.</value>
        public string ConsoleDescription { get; protected set; }


        /// <summary>
        /// Thens the select and execute.
        /// </summary>
        /// <param name="consoleArguments">The console arguments.</param>
        public void ThenSelectAndExecute(IList<string> consoleArguments)
        {
            if (consoleArguments.Count > 0)
            {
                string commandName = consoleArguments[0].ToLower();
                IConsoleCommand command;
                if (_registeredCommands.TryGetValue(commandName, out command))
                {
                    command.Execute(consoleArguments.ToList());
                    return;
                }
            }
            ShowHelp();
        }

        /// <summary>
        /// Shows the help.
        /// </summary>
        protected virtual void ShowHelp()
        {
            var entryAsm = Assembly.GetEntryAssembly();
            Console.DisplayMessage($"{entryAsm.GetName().Name} - V.{entryAsm.GetName().Version}");
            if (!string.IsNullOrWhiteSpace(this.ConsoleDescription))
            {
                Console.DisplayMessage(string.Empty);
                Console.DisplayMessage(this.ConsoleDescription);
            }
            Console.DisplayMessage(string.Empty);
            Console.DisplayMessage("Utilizzo:");
            Console.DisplayMessage($"\t{entryAsm.GetName().Name}.exe <command-name> <command-arguments>");
            Console.DisplayMessage($"\t{entryAsm.GetName().Name}.exe <command-name> h|help");
            Console.DisplayMessage(string.Empty);
            Console.DisplayMessage("Comandi disponibili:");
            foreach (var registeredCommand in _registeredCommands)
            {
                Console.DisplayMessage($"\t- {registeredCommand.Key}");
            }
        }


        /// <summary>
        /// Registers the command.
        /// </summary>
        /// <typeparam name="TSelector">The type of the t selector.</typeparam>
        /// <param name="commandName">Name of the command.</param>
        /// <returns>CommandSelector.</returns>
        public CommandSelector RegisterCommand<TSelector>(string commandName)
            where TSelector : IConsoleCommand, new()
        {
            var command = new TSelector();
            command.Console = this.Console;
            _registeredCommands[commandName.ToLower()] = command;
            return this;
        }
    }
}
