using System;
using System.Windows.Input;

namespace AlarmMonitor.Infrastructure
{
    /// <summary>
    /// Implementazione di ICommand.
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class RelayCommand : ICommand
    {

        private readonly Action<object> handler;
        private bool isEnabled;
        private readonly Func<object,bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="canExecute">The can execute.</param>
        public RelayCommand(Action<object> handler, Func<object,bool> canExecute = null)
        {
            this.handler = handler;
            this.canExecute = canExecute;
            if (canExecute == null)
                isEnabled = true;
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value != isEnabled)
                {
                    isEnabled = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            if (canExecute != null)
                IsEnabled = canExecute(parameter);

            return IsEnabled;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            handler(parameter);
        }

        /// <summary>
        /// Method used to raise the <see cref="CanExecuteChanged" /> event
        /// to indicate that the return value of the <see cref="CanExecute" />
        /// method has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    //public class RelayCommand<T> : ICommand
    //{
    //    private readonly Action<T> handler;
    //    private bool isEnabled = true;

    //    private readonly Func<T, bool> canExecute;

    //    public RelayCommand(Action<T> handler, Func<T, bool> canExecute = null)
    //    {
    //        this.handler = handler;
    //        this.canExecute = canExecute;
    //        if (canExecute == null)
    //            isEnabled = true;
    //    }

    //    public bool IsEnabled
    //    {
    //        get { return isEnabled; }
    //        set
    //        {
    //            if (value != isEnabled)
    //            {
    //                isEnabled = value;
    //                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    //            }
    //        }
    //    }

    //    public bool CanExecute(object parameter)
    //    {
    //        if (canExecute != null)
    //            IsEnabled = canExecute((T)parameter);

    //        return IsEnabled;
    //    }

    //    public event EventHandler CanExecuteChanged;

    //    public void Execute(object parameter)
    //    {
    //        handler((T)parameter);
    //    }

    //    /// <summary>
    //    /// Method used to raise the <see cref="CanExecuteChanged"/> event
    //    /// to indicate that the return value of the <see cref="CanExecute"/>
    //    /// method has changed.
    //    /// </summary>
    //    public void RaiseCanExecuteChanged()
    //    {
    //        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    //    }
    //}

}
