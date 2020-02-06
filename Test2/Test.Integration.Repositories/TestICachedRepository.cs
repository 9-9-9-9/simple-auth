using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Repositories;
using Test.Shared;

namespace Test.Integration.Repositories
{
    public class TestICachedRepository : BaseTestClass
    {
        private ICachedRepository<string> Svc => new MemoryCachedRepository<string>();

        private Task<T> Get<T>(string key, ICachedRepository<T> cachedRepository)
            where T : class => cachedRepository.GetAsync(key, "c", "a");

        private Task Push<T>(string key, T val, ICachedRepository<T> cachedRepository)
            where T : class => cachedRepository.PushAsync(val, key, "c", "a");

        private Task Clear<T>(ICachedRepository<T> cachedRepository)
            where T : class => cachedRepository.ClearAsync("c", "a");

        [Test]
        public async Task Push()
        {
            var memorySvc = Svc;
            await Push("k", "v1", memorySvc);
            Assert.AreEqual("v1", await Get("k", memorySvc));
            await Push("k", "v2", memorySvc);
            Assert.AreEqual("v2", await Get("k", memorySvc));
            Assert.CatchAsync<ArgumentNullException>(async() => await memorySvc.PushAsync("v", string.Empty, "c", "a"));
            Assert.CatchAsync<ArgumentNullException>(async() => await memorySvc.PushAsync("v", "k", string.Empty, "a"));
            Assert.CatchAsync<ArgumentNullException>(async() => await memorySvc.PushAsync("v", "k", "c", null));

            // due to allowing string.Empty as value of app, the following statement would not throwing exception
            await memorySvc.PushAsync("v", "k", "c", string.Empty);
        }

        [Test]
        public async Task Get()
        {
            var memorySvc = Svc;
            await Push("k1", "v", memorySvc);
            Assert.NotNull(await Get("k1", memorySvc));
            Assert.IsNull(await Get("k2", memorySvc));
            Assert.CatchAsync<ArgumentNullException>(async() => await memorySvc.GetAsync(string.Empty, "c", "a"));
            Assert.CatchAsync<ArgumentNullException>(async() => await memorySvc.GetAsync("k", string.Empty, "a"));
            Assert.CatchAsync<ArgumentNullException>(async() => await memorySvc.GetAsync("k", "c", null));

            // due to allowing string.Empty as value of app, the following statement would not throwing exception
            await memorySvc.GetAsync("k", "c", string.Empty);
        }

        [Test]
        public async Task Clear()
        {
            var memorySvc = Svc;
            await Push("k1", "v", memorySvc);
            await Push("k2", "v", memorySvc);
            await Push("k3", "v", memorySvc);

            Assert.CatchAsync<ArgumentNullException>(async () => await memorySvc.ClearAsync(null, "a"));
            Assert.CatchAsync<ArgumentNullException>(async () => await memorySvc.ClearAsync(string.Empty, "a"));
            Assert.CatchAsync<ArgumentNullException>(async () => await memorySvc.ClearAsync("c", null));
            await memorySvc.ClearAsync("c", string.Empty);
            Assert.NotNull(await Get("k1", memorySvc));
            Assert.NotNull(await Get("k2", memorySvc));
            Assert.NotNull(await Get("k3", memorySvc));
            
            await Clear(memorySvc);
            Assert.IsNull(await Get("k1", memorySvc));
            Assert.IsNull(await Get("k2", memorySvc));
            Assert.IsNull(await Get("k3", memorySvc));
        }
    }
}