﻿using System.Diagnostics.CodeAnalysis;
using Caliban.Nano.Contracts;

namespace Caliban.Nano.Events
{
    /// <summary>
    /// An thread-safe event aggregator.
    /// </summary>
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly List<(Type, object)> _subscriptions = new();

        /// <summary>
        /// Clears all subscriptions on dispose.
        /// </summary>
        public void Dispose()
        {
            lock (_subscriptions)
            {
                _subscriptions.Clear();
            }
        }

        /// <inheritdoc />
        public bool HasHandler<T>()
        {
            lock (_subscriptions)
            {
                return _subscriptions.Any(s => s.Item1 == typeof(T));
            }
        }

        /// <inheritdoc />
        public void Subscribe<T>([NotNull] object handler)
        {
            lock (_subscriptions)
            {
                _subscriptions.Add(new(typeof(T), handler));
            }
        }

        /// <inheritdoc />
        public void Unsubscribe<T>([NotNull] object handler)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(new(typeof(T), handler));
            }
        }

        /// <inheritdoc />
        public void Publish<T>(T message) where T : notnull
        {
            IEnumerable<object> handlers;

            lock (_subscriptions)
            {
                handlers = _subscriptions
                    .Where(s => s.Item1 == typeof(T))
                    .Select(s => s.Item2);
            }

            foreach (var handler in handlers)
            {
                switch (handler)
                {
                    case IHandle<T> handle:
                        handle.Handle(message);
                        break;

                    case Action<T> action:
                        action.Invoke(message);
                        break;

                    default:
                        Log.Intern($"Handler {handler.GetType().Name} is not supported");
                        break;
                }
            }
        }
    }
}