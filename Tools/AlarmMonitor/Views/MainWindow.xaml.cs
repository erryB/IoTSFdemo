using System.Windows;
using AlarmMonitor.ViewModels;

namespace AlarmMonitor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase<MainWindowViewModel>
    {
        public MainWindow()
        {
            this.Closing += MainWindow_Closing;
            InitializeComponent();
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await this.ViewModel.CloseAsync();
        }
    }
}
