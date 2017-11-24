using Autofac;

namespace AlarmMonitor.ViewModels
{
    public sealed class ViewModelLocator
    {

        public MainWindowViewModel MainWindowViewModel => App.Container.Resolve<MainWindowViewModel>();

    }
}
