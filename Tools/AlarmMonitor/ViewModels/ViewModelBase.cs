using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using AlarmMonitor.Infrastructure;
using AlarmMonitor.Messages;
using AlarmMonitor.Models;
using AlarmMonitor.Views;

namespace AlarmMonitor.ViewModels
{
   
    public abstract class ViewModelBase : NotifyObject
    {
       
        protected ViewModelBase(IEventAggregator eventAggregator,
            INavigationService navigationService)
        {
            if (eventAggregator == null)
                throw new ArgumentNullException(nameof(eventAggregator));
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            this.NavigationService = navigationService;
            this.EventAggregator = eventAggregator;
            this.IsBusy = false;
        }


        protected readonly IEventAggregator EventAggregator;
        protected readonly INavigationService NavigationService;


        protected abstract ViewsEnum GetViewType();

        public virtual Task InitializeAsync()
        {
            return Task.Delay(0);
        }

        protected void OnShowMessagebox(ShowMessageBoxEventArgs e)
        {
            ShowMessageBox?.Invoke(this, e);
        }

        public event EventHandler<ShowMessageBoxEventArgs> ShowMessageBox;

        public virtual void Close()
        {
            NavigationService.Close(GetViewType());
        }
        public bool IsInDesignMode
        {
            get
            {
                return (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
            }
        }
        protected virtual void OpenWindow(ViewsEnum view)
        {
            NavigationService.Open(view, null);
        }

        /// <summary>
        /// Indica se il view model è occupato in operazioni di lunga durata.
        /// </summary>
        /// <value><c>true</c> se il view model sta eseguendo un'operazione; altrimenti, <c>false</c>.</value>
        /// <remarks>Da utilizzare per visualizzare eventuali progressbar di avanzamento operazioni.</remarks>
        public bool IsBusy
        {
            get { return GetPropertyValue<bool>(); }
            set { SetPropertyValue(value); }
        }

        #region [ Command ] 

        public RelayCommand _ExitCommand;
        public RelayCommand ExitCommand
        {
            get
            {
                if (_ExitCommand == null)
                    _ExitCommand = new RelayCommand(ExitExecute, ExitCanExecute);
                return _ExitCommand;
            }
        }

        protected virtual void ExitExecute(object parameter)
        {
            // Close window
            EventAggregator.Publish(new CloseWindowMessage(this.GetViewType()));
        }

        protected virtual bool ExitCanExecute(object parameter)
        {
            return true;
        }

        #endregion [ Command ]

    }


}
