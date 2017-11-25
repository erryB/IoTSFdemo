using System.Windows;
using AlarmMonitor.Infrastructure;
using AlarmMonitor.Services;
using AlarmMonitor.ViewModels;
using Autofac;

namespace AlarmMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherHelper.Initialize();
        }

        private static IContainer _Container;
        public static IContainer Container
        {
            get
            {
                if (_Container == null)
                {
                    var builder = new ContainerBuilder();
                    ConfigureContainer(builder);
                    _Container = builder.Build();
                }
                return _Container;
            }
        }

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<AppConfigAdapter>().As<IConfigurationAdapter>().InstancePerDependency();
            builder.RegisterType<ServiceBusAlarmReceiver>().As<IAlarmReceiver>().InstancePerDependency();

            // Infrastructure
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();

            //ViewModels
            builder.RegisterType<MainWindowViewModel>();

        }
    }
}
