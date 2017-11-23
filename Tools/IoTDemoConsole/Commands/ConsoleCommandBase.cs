using System.Collections.Generic;
using System.Linq;
using IoTDemoConsole.Attributes;
using IoTDemoConsole.Helpers;
using IoTDemoConsole.Interfaces;
using NDesk.Options;

namespace IoTDemoConsole.Commands
{

    /// <summary>
    /// Class ConsoleCommandBase.
    /// </summary>
    /// <typeparam name="TArguments">The type of the t arguments.</typeparam>
    /// <seealso cref="IoTDemoConsole.Interfaces.IConsoleCommand" />
    public abstract class ConsoleCommandBase<TArguments> : IConsoleCommand
        where TArguments : class, new()
    {


        /// <summary>
        /// Class HelpCommandParameters.
        /// </summary>
        private class HelpCommandParameters
        {

            /// <summary>
            /// Gets or sets a value indicating whether [show help].
            /// </summary>
            /// <value><c>true</c> if [show help]; otherwise, <c>false</c>.</value>
            public bool ShowHelp { get; set; }
        }

        /// <summary>
        /// Configures the help option set.
        /// </summary>
        /// <param name="helpArguments">The help arguments.</param>
        /// <returns>OptionSet.</returns>
        private OptionSet ConfigureHelpOptionSet(HelpCommandParameters helpArguments)
        {
            return new OptionSet() { { "h|help", v => helpArguments.ShowHelp = v != null } };
        }

        /// <summary>
        /// Configures the option set.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>OptionSet.</returns>
        protected virtual OptionSet ConfigureOptionSet(TArguments arguments)
        {
            return CommandParameterHelper.CreateOptionSetFromArguments(arguments);
        }

        /// <summary>
        /// Shows the command description.
        /// </summary>
        protected virtual void ShowCommandDescription()
        {
        }

        /// <summary>
        /// Esegue il comando passando la lista di argomenti delal console.
        /// </summary>
        /// <param name="arguments">Lista degli argomenti della console.</param>
        public void Execute(IList<string> arguments)
        {
            var argumentsModel = new TArguments();
            var optionSet = ConfigureOptionSet(argumentsModel);
            var helpArgumentsModel = new HelpCommandParameters();
            var helpOptionSet = ConfigureHelpOptionSet(helpArgumentsModel);
            try
            {
                optionSet.Parse(arguments);
            }
            catch (OptionException optEx)
            {
                this.DisplayError("Gli argomenti del comando sono errati");
                this.DisplayError(optEx.Message);
                optionSet.WriteOptionDescriptions(Console.Out);
            }
            try
            {
                helpOptionSet.Parse(arguments);
            }
            catch
            {
                helpArgumentsModel.ShowHelp = false;
            }

            if (helpArgumentsModel.ShowHelp)
            {
                this.DisplayMessage(string.Empty);
                ShowCommandDescription();
                this.DisplayMessage(string.Empty);
                this.DisplayMessage("Comandi disponibili: ");
                optionSet.WriteOptionDescriptions(Console.Out);
            }
            else
            {
                if (AreArgumentsOk(argumentsModel))
                {
                    if (!this.Execute(argumentsModel))
                    {
                        ShowCommandDescription();
                        this.DisplayMessage(string.Empty);
                        this.DisplayMessage("Parametri del comando: ");
                        optionSet.WriteOptionDescriptions(Console.Out);
                    }
                }
            }
            this.DisplayMessage(string.Empty);
        }

        /// <summary>
        /// Imposta o recupera l'istanza di <see cref="IConsoleOutput" /> utilizzata per ridirigere l'output del comando.
        /// </summary>
        /// <value>The console.</value>
        public IConsoleOutput Console { get; set; }

        /// <summary>
        /// Ares the arguments ok.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected virtual bool AreArgumentsOk(TArguments arguments)
        {
            var errori = CommandParameterHelper.CheckParameters<TArguments>(arguments);
            if (errori != null && errori.Any())
            {
                foreach (var errore in errori)
                {
                    this.DisplayError(errore);
                    this.DisplayMessage(string.Empty);
                }
                return false;
            }
            return true;
        }


        /// <summary>
        /// Executes the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected abstract bool Execute(TArguments arguments);


        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        protected virtual void DisplayError(string message)
        {
            this.Console?.DisplayError(message);
        }

        /// <summary>
        /// Displays the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        protected virtual void DisplayWarning(string message)
        {
            this.Console?.DisplayWarning(message);
        }

        /// <summary>
        /// Displays the result.
        /// </summary>
        /// <param name="message">The message.</param>
        protected virtual void DisplayResult(string message)
        {
            this.Console?.DisplayResult(message);
        }

        /// <summary>
        /// Displays the message.
        /// </summary>
        /// <param name="message">The message.</param>
        protected virtual void DisplayMessage(string message)
        {
            this.Console?.DisplayMessage(message);
        }
    }
}
