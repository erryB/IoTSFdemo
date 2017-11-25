using System;

namespace AlarmMonitor.Infrastructure
{
    public interface ISubscription<TMessage> : IDisposable where TMessage : IMessage
    {
        Action<TMessage> Action { get; }
        IEventAggregator EventAggregator { get; }
    }

    public class Subscription<TMessage> : ISubscription<TMessage> where TMessage : IMessage
    {
        public Action<TMessage> Action
        {
            get { return m_Action; }
            private set { m_Action = value; }
        }

        private Action<TMessage> m_Action;
        public IEventAggregator EventAggregator
        {
            get { return m_EventAggregator; }
            private set { m_EventAggregator = value; }
        }


        private IEventAggregator m_EventAggregator;
        public Subscription(IEventAggregator eventAggregator, Action<TMessage> action)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.EventAggregator = eventAggregator;
            this.Action = action;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EventAggregator.UnSubscribe(this);
            }
        }

    }

}
