using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SimpleAuth.Shared.Extensions;

namespace SimpleAuth.Repositories
{
    public interface ICachedRepository<T> where T : class
    {
        Task PushAsync(T obj, string key, string corp, string app);
        Task<T> GetAsync(string key, string corp, string app);
        Task ClearAsync(string corp, string app);
    }

    public interface IMemoryCachedRepository<T> : ICachedRepository<T> where T : class
    {
    }

    public class MemoryCachedRepository<T> : IMemoryCachedRepository<T> where T : class
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, T>> _cache =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, T>>();

        public async Task PushAsync(T obj, string key, string corp, string app)
        {
            await Task.CompletedTask;
            ThrowIfBlank(key, corp, app);
            
            _cache
                .GetOrAdd(Key(corp, app), s => new ConcurrentDictionary<string, T>())
                .AddOrUpdate(key, s => obj, (s2, o) => obj);
        }

        public async Task<T> GetAsync(string key, string corp, string app)
        {
            await Task.CompletedTask;
            ThrowIfBlank(key, corp, app);
            
            if (_cache
                .GetOrAdd(Key(corp, app), s => new ConcurrentDictionary<string, T>())
                .TryGetValue(key, out var res))
                return res;
            
            return null;
        }

        public async Task ClearAsync(string corp, string app)
        {
            await Task.CompletedTask;
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
            if (app == null)
                throw new ArgumentNullException(nameof(app));
        }

        private void ThrowIfBlank(string corp, string app)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            if (app == null)
                throw new ArgumentNullException(nameof(app));
        }

        protected virtual string Key(string corp, string app) => $"{corp}.{app}";
    }
}