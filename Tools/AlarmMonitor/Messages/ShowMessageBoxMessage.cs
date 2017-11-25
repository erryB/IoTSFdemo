using AlarmMonitor.Infrastructure;

namespace AlarmMonitor.Messages
{
    public class ShowMessageBoxMessage : IMessage
    {
        public ShowMessageBoxMessage(string messageBoxName)
        {
            this.messageBoxName = messageBoxName;
        }

        private string messageBoxName;
        public string MessageBoxName => messageBoxName;

    }
}
