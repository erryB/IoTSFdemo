using AlarmMonitor.Views;

namespace AlarmMonitor.Infrastructure
{
    public interface INavigationService
    {
        void Open(ViewsEnum view, params object[] arguments);

        void Close(ViewsEnum currentView);
    }
}
