using System;
using System.Windows;
using AlarmMonitor.Infrastructure;
using AlarmMonitor.ViewModels;
using Autofac;

namespace AlarmMonitor.Views
{
    public abstract class WindowBase<TViewModel> : Window where TViewModel : ViewModelBase
    {
        protected WindowBase()
        {
            EventAggregator = App.Container.Resolve<IEventAggregator>();
            this.Initialized += WindowBase_Initialized;
        }

        private async void WindowBase_Initialized(object sender, EventArgs e)
        {
            if (this.ViewModel != null && !this.ViewModel.IsInDesignMode)
            {
                await this.ViewModel.InitializeAsync();
                this.ViewModel.ShowMessageBox += ViewModel_ShowMessageBox;
            }
        }
        protected virtual void ViewModel_ShowMessageBox(object sender, ShowMessageBoxEventArgs e)
        {

        }

        protected readonly IEventAggregator EventAggregator;
        protected TViewModel ViewModel => this.DataContext as TViewModel;

    }
}
