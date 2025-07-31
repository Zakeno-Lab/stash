using System;

namespace synapse.Services
{
    /// <summary>
    /// Event bus interface for decoupled communication between services
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes an event to all subscribers
        /// </summary>
        /// <typeparam name="T">The event type</typeparam>
        /// <param name="eventData">The event data to publish</param>
        void Publish<T>(T eventData) where T : class;

        /// <summary>
        /// Subscribes to events of a specific type
        /// </summary>
        /// <typeparam name="T">The event type to subscribe to</typeparam>
        /// <param name="handler">The handler to call when the event is published</param>
        /// <returns>A subscription that can be disposed to unsubscribe</returns>
        IDisposable Subscribe<T>(Action<T> handler) where T : class;
    }
} 