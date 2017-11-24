using AlarmMonitor.Infrastructure;
using AlarmMonitor.Views;

namespace AlarmMonitor.Messages
{
    public class CloseWindowMessage : IMessage
    {
        public CloseWindowMessage(ViewsEnum closedView)
        {
            this.closedView = closedView;
        }

        private ViewsEnum closedView;
        public ViewsEnum ClosedView => closedView;

    }
}
