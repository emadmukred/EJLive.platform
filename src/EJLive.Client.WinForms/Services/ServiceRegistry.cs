using System;
using System.Collections.Concurrent;

namespace EJLive.Client.WinForms.Services
{
    public static class ServiceRegistry
    {
        private static readonly ConcurrentDictionary<Type, object> _map = new();

        public static void Register<T>(T instance) where T : class
        {
            _map[typeof(T)] = instance!;
        }

        public static T? Get<T>() where T : class
        {
            if (_map.TryGetValue(typeof(T), out var o))
                return o as T;
            return null;
        }
    }
}
