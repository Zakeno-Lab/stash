using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace synapse.Services
{
    /// <summary>
    /// Thread-safe event bus implementation for decoupled communication
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _subscribers = new();
        private readonly object _lock = new object();

        public void Publish<T>(T eventData) where T : class
        {
            if (eventData == null)
                return;

            var eventType = typeof(T);
            
            if (_subscribers.TryGetValue(eventType, out var subscribers))
            {
                var handlersToCall = new List<Action<T>>();
                
                // Collect all handlers first to avoid holding the lock during execution
                foreach (var subscriber in subscribers)
                {
                    if (subscriber is Action<T> handler)
                    {
                        handlersToCall.Add(handler);
                    }
                }
                
                // Execute handlers outside the lock
                foreach (var handler in handlersToCall)
                {
                    try
                    {
                        handler(eventData);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't stop other handlers from executing
                        System.Diagnostics.Debug.WriteLine($"EventBus: Error in event handler: {ex.Message}");
                    }
                }
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(T);
            
            lock (_lock)
            {
                if (!_subscribers.TryGetValue(eventType, out var subscribers))
                {
                    subscribers = new ConcurrentBag<object>();
                    _subscribers[eventType] = subscribers;
                }
                
                subscribers.Add(handler);
            }

            return new Subscription(() => UnsubscribeHandler(eventType, handler));
        }

        private void UnsubscribeHandler<T>(Type eventType, Action<T> handler) where T : class
        {
            lock (_lock)
            {
                if (_subscribers.TryGetValue(eventType, out var subscribers))
                {
                    // ConcurrentBag doesn't support removal, so we create a new bag without the handler
                    var newSubscribers = new ConcurrentBag<object>();
                    
                    foreach (var subscriber in subscribers)
                    {
                        if (!ReferenceEquals(subscriber, handler))
                        {
                            newSubscribers.Add(subscriber);
                        }
                    }
                    
                    _subscribers[eventType] = newSubscribers;
                }
            }
        }

        private class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            private bool _disposed = false;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _unsubscribe?.Invoke();
                    _disposed = true;
                }
            }
        }
    }
} 