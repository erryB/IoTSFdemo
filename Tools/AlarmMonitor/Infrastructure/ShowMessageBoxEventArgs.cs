using System;
using System.Windows;

namespace AlarmMonitor.Infrastructure
{
    public class ShowMessageBoxEventArgs : EventArgs
    {
        public string Message { get; set; }

        public string  Title { get; set; }

        public MessageBoxButton Buttons { get; set; }

        public MessageBoxImage Image { get; set; }

        public string MessageCode { get; set; }
    }
}
