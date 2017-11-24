using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlarmMonitor.Infrastructure;
using AlarmMonitor.Messages;
using AlarmMonitor.Models;
using AlarmMonitor.Services;
using AlarmMonitor.Views;

namespace AlarmMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(IEventAggregator eventAggregator,
            INavigationService navigationService, IAlarmReceiver alarmReceiver) : base(eventAggregator, navigationService)
        {
            if (alarmReceiver == null)
                throw new ArgumentNullException(nameof(alarmReceiver));

            this.alarmReceiver = alarmReceiver;
        }

        public async Task CloseAsync()
        {
            CloseViewCancellationTokenSource.Cancel();
            this.alarmReceiver.AlarmMessageReceived -= AlarmReceiver_AlarmMessageReceived;
            await this.alarmReceiver.StopReceiverAsync();
        }

        private readonly IAlarmReceiver alarmReceiver;
        private Task AlarmedDeviceRemoval;
        private CancellationTokenSource CloseViewCancellationTokenSource;


        protected override ViewsEnum GetViewType() => ViewsEnum.MainWindow;

        public override async Task InitializeAsync()
        {
            AlarmedDevices = new ObservableCollection<DeviceInfo>();
            this.alarmReceiver.AlarmMessageReceived += AlarmReceiver_AlarmMessageReceived;
            await this.alarmReceiver.StartReceiverAsync();
            CloseViewCancellationTokenSource = new CancellationTokenSource();
            AlarmedDeviceRemoval = new Task(() => AlarmedDeviceRemovalCode(CloseViewCancellationTokenSource.Token));
            AlarmedDeviceRemoval.Start();
        }

        private void AlarmedDeviceRemovalCode(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                for (int i = 0; i < AlarmedDevices.Count && !cancellationToken.IsCancellationRequested; i++)
                {
                    var alarmedDevice = AlarmedDevices.ElementAt(i);
                    if (alarmedDevice.IsToBeRemoved())
                    {
                        AlarmedDevices.RemoveAt(i);
                        i--;
                    }
                }
                Task.Delay(10000, cancellationToken);
            }
        }

        private void AlarmReceiver_AlarmMessageReceived(object sender, AlarmMessage e)
        {
            var device = AlarmedDevices.FirstOrDefault(d => d.DeviceId == e.DeviceId);
            if (device != null)
            {
                device.LastAlarmMessage = e;
            }
            else
            {
                AlarmedDevices.Add(new DeviceInfo() { DeviceId = e.DeviceId, LastAlarmMessage = e });
            }
        }

        public ObservableCollection<DeviceInfo> AlarmedDevices
        {
            get { return GetPropertyValue<ObservableCollection<DeviceInfo>>(); }
            set { SetPropertyValue(value); }
        }

    }

}
