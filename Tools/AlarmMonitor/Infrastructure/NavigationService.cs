using System;
using AlarmMonitor.Messages;
using AlarmMonitor.Views;

namespace AlarmMonitor.Infrastructure
{
    public class NavigationService : INavigationService
    {
        public NavigationService(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null) throw new ArgumentNullException(nameof(eventAggregator));
            EventAggregator = eventAggregator;
        }

        private IEventAggregator EventAggregator;

        public void Close(ViewsEnum currentView)
        {
            EventAggregator.Publish<CloseWindowMessage>(new CloseWindowMessage(currentView));
        }

        public void Open(ViewsEnum view, params object[] arguments)
        {
            EventAggregator.Publish<OpenWindowMessage>(new OpenWindowMessage(view));
        }
    }
}
