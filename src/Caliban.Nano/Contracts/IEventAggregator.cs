﻿using System.Diagnostics.CodeAnalysis;

namespace Caliban.Nano.Contracts
{
    /// <summary>
    /// An interface for a type separated event aggregator.
    /// </summary>
    public interface IEventAggregator : IDisposable
    {
        /// <summary>
        /// Returns if a handler is subscribed to the type.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <returns>True if the type has a handler; otherwise false.</returns>
        bool HasHandler<T>() where T : notnull;

        /// <summary>
        /// Subscribes a handler to a type.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="handler">The event handler.</param>
        void Subscribe<T>([NotNull] object handler) where T : notnull;

        /// <summary>
        /// Unsubscribes a handler from a type.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="handler">The event handler.</param>
        void Unsubscribe<T>([NotNull] object handler) where T : notnull;

        /// <summary>
        /// Publishes a message to all subscribed handlers.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="message">The message.</param>
        void Publish<T>(T message) where T : notnull;
    }
}
