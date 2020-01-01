using System;
using System.Collections.Concurrent;
using SimpleAuth.Core.Extensions;

namespace SimpleAuth.Repositories
{
    public interface ICachedRepository<T> where T : class
    {
        void Push(T obj, string key, string corp, string app);
        T Get(string key, string corp, string app);
        void Clear(string corp, string app);
    }

    public interface IMemoryCachedRepository<T> : ICachedRepository<T> where T : class
    {
    }

    public class MemoryCachedRepository<T> : IMemoryCachedRepository<T> where T : class
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, T>> _cache =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, T>>();

        public void Push(T obj, string key, string corp, string app)
        {
            ThrowIfBlank(key, corp, app);
            _cache
                .GetOrAdd(Key(corp, app), s => new ConcurrentDictionary<string, T>())
                .AddOrUpdate(key, s => obj, (s2, o) => obj);
        }

        public T Get(string key, string corp, string app)
        {
            ThrowIfBlank(key, corp, app);
            if (_cache
                .GetOrAdd(Key(corp, app), s => new ConcurrentDictionary<string, T>())
                .TryGetValue(key, out var res))
                return res;
            return null;
        }

        public void Clear(string corp, string app)
        {
            ThrowIfBlank(corp, app);
            _cache
                .GetOrAdd(Key(corp, app), s => new ConcurrentDictionary<string, T>()).Clear();
        }

        private void ThrowIfBlank(string key, string corp, string app)
        {
            if (key.IsBlank())
                throw new ArgumentNullException(nameof(key));
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));
        }

        private void ThrowIfBlank(string corp, string app)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));
        }

        protected virtual string Key(string corp, string app) => $"{corp}.{app}";
    }
}