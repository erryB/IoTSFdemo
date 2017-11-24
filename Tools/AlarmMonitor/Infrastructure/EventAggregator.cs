﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AlarmMonitor.Infrastructure
{
    public interface IEventAggregator
    {
        void Publish<TMessage>(TMessage message) where TMessage : IMessage;
        ISubscription<TMessage> Subscribe<TMessage>(Action<TMessage> action) where TMessage : IMessage;

        void UnSubscribe<TMessage>(ISubscription<TMessage> subscription) where TMessage : IMessage;
        void ClearAllSubscriptions();
        void ClearAllSubscriptions(Type[] exceptMessages);
    }

    public class EventAggregator : IEventAggregator
    {

        private readonly IDictionary<Type, IList> _subscriptions = new Dictionary<Type, IList>();

        public EventAggregator()
        {
        }

        public void Publish<TMessage>(TMessage message) where TMessage : IMessage
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Type messageType = typeof(TMessage);
            if (_subscriptions.ContainsKey(messageType))
            {
                var subscriptionList = new List<ISubscription<TMessage>>(_subscriptions[messageType].Cast<ISubscription<TMessage>>());
                foreach (var subscription in subscriptionList)
                    subscription.Action.Invoke(message);
            }
        }

        public ISubscription<TMessage> Subscribe<TMessage>(Action<TMessage> action) where TMessage : IMessage
        {
            Type messageType = typeof(TMessage);
            dynamic subscription = new Subscription<TMessage>(this, action);

            if (_subscriptions.ContainsKey(messageType))
                _subscriptions[messageType].Add(subscription);
            else
                _subscriptions.Add(messageType, new List<ISubscription<TMessage>> { subscription });

            return subscription;
        }

        public void UnSubscribe<TMessage>(ISubscription<TMessage> subscription) where TMessage : IMessage
        {
            Type messageType = typeof(TMessage);
            if (_subscriptions.ContainsKey(messageType))
                _subscriptions[messageType].Remove(subscription);
        }

        public void ClearAllSubscriptions()
        {
            ClearAllSubscriptions(null);
        }

        public void ClearAllSubscriptions(Type[] exceptMessages)
        {
            foreach (var messageSubscriptions in new Dictionary<Type, IList>(_subscriptions))
            {
                bool canDelete = true;
                if (exceptMessages != null)
                    canDelete = !exceptMessages.Contains(messageSubscriptions.Key);

                if (canDelete)
                    _subscriptions.Remove(messageSubscriptions);
            }
        }
    }


}
