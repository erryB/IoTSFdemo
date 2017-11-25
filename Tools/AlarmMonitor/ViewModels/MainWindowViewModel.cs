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
            UserMessage = "Initializing ServiceBusReceiver....";
            AlarmedDevices = new ObservableCollection<DeviceInfo>();
            this.alarmReceiver.AlarmMessageReceived += AlarmReceiver_AlarmMessageReceived;
            await this.alarmReceiver.StartReceiverAsync();
            CloseViewCancellationTokenSource = new CancellationTokenSource();
            AlarmedDeviceRemoval = new Task(() => AlarmedDeviceRemovalCode(CloseViewCancellationTokenSource.Token));
            AlarmedDeviceRemoval.Start();
            UserMessage = "Initialized ServiceBusReceiver....";
            await Task.Delay(2000);
            UserMessage = null;
        }

        private void AlarmedDeviceRemovalCode(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                for (int i = AlarmedDevices.Count - 1; i >= 0; i--)
                {
                    if (AlarmedDevices[i].IsToBeRemoved())
                    {
                        AlarmedDevices.RemoveAt(i);
                    }
                }


                Task.Delay(10000, cancellationToken);
            }
        }

        private void AlarmReceiver_AlarmMessageReceived(object sender, AlarmMessage e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                UserMessage = $"[{DateTime.Now:HH:mm:ss}] Received message from device {e.DeviceId} - {e.Message}";
                var device = AlarmedDevices.FirstOrDefault(d => d.DeviceId == e.DeviceId);
                if (device != null)
                {
                    device.LastAlarmMessage = e;
                }
                else
                {
                    AlarmedDevices.Add(new DeviceInfo() { DeviceId = e.DeviceId, LastAlarmMessage = e });
                }
            });
        }

        public ObservableCollection<DeviceInfo> AlarmedDevices
        {
            get => GetPropertyValue<ObservableCollection<DeviceInfo>>();
            set => SetPropertyValue(value);
        }

        public string UserMessage
        {
            get => GetPropertyValue<string>();
            set => SetPropertyValue(value);
        }
    }

}
