using AlarmMonitor.Infrastructure;
using AlarmMonitor.Views;

namespace AlarmMonitor.Messages
{
    public class OpenWindowMessage : IMessage
    {
        public OpenWindowMessage(ViewsEnum openView)
        {
            this.openView = openView;
        }

        private ViewsEnum openView;
        public ViewsEnum OpenView => openView;

    }
}
