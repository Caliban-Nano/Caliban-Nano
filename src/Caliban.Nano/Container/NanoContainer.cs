﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Caliban.Nano.Contracts;
using Caliban.Nano.Exceptions;

namespace Caliban.Nano.Container
{
    /// <summary>
    /// A thread-safe minimal dependency injection container.
    /// </summary>
    public sealed class NanoContainer : IContainer
    {
        /// <inheritdoc />
        public event Action<object>? Resolved = null;

        private readonly Dictionary<Type, object> _storage = new();

        /// <summary>
        /// Initializes a new self registered instance of this class.
        /// </summary>
        public NanoContainer()
        {
            Register<IContainer>(this);
        }

        /// <summary>
        /// Clears the container storage on dispose.
        /// </summary>
        public void Dispose()
        {
            lock (_storage)
            {
                _storage.Clear();
            }
        }

        /// <inheritdoc />
        public object Resolve<T>()
        {
            return Resolve(typeof(T));
        }

        /// <inheritdoc />
        public object Resolve([NotNull] object request)
        {
            object? instance = request;

            // Resolve or create an instance if the request is a type

            if (request is Type type)
            {
                if (!GetValue(type, out instance) || instance is null)
                {
                    var ctors = type.GetConstructors();

                    if (ctors.Length != 1)
                    {
                        throw new NanoContainerException($"Type {type.Name} has more than one constructor");
                    }

                    var ctor = ctors.Single();

                    instance = ctor.Invoke(GetParameters(ctor));

                    if (instance is null)
                    {
                        throw new NanoContainerException($"Type {type.Name} could not be created");
                    }
                }
            }

            // Inject properties into the instance

            var properties = instance.GetType().GetProperties();

            foreach (var property in properties.Where(p => p.CanWrite))
            {
                if (GetValue(property.PropertyType, out var value))
                {
                    property.SetValue(instance, value);
                }
            }

            Resolved?.Invoke(instance);

            return instance;
        }

        /// <inheritdoc />
        public void Register<T>([NotNull] object instance)
        {
            lock (_storage)
            {
                _storage.Add(typeof(T), instance);
            }
        }

        /// <inheritdoc />
        public bool IsRegistered<T>()
        {
            lock (_storage)
            {
                return _storage.ContainsKey(typeof(T));
            }
        }

        /// <inheritdoc />
        public void Unregister<T>()
        {
            lock (_storage)
            {
                _storage.Remove(typeof(T));
            }
        }

        private bool GetValue(Type type, out object? value)
        {
            lock (_storage)
            {
                return _storage.TryGetValue(type, out value);
            }
        }

        private object?[] GetParameters(ConstructorInfo info)
        {
            return info.GetParameters()
                .Select(p => GetValue(p.ParameterType, out var value) ? value : null)
                .ToArray();
        }
    }
}