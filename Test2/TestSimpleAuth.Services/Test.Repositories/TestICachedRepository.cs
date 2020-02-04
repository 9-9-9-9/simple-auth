using System;
using NUnit.Framework;
using SimpleAuth.Repositories;
using Test.Shared;

namespace Test.SimpleAuth.Services.Test.Repositories
{
    public class TestICachedRepository : BaseTestClass
    {
        private ICachedRepository<string> Svc => new MemoryCachedRepository<string>();

        private T Get<T>(string key, ICachedRepository<T> cachedRepository)
            where T : class => cachedRepository.Get(key, "c", "a");

        private void Push<T>(string key, T val, ICachedRepository<T> cachedRepository)
            where T : class => cachedRepository.Push(val, key, "c", "a");

        private void Clear<T>(ICachedRepository<T> cachedRepository)
            where T : class => cachedRepository.Clear("c", "a");

        [Test]
        public void Push()
        {
            var memorySvc = Svc;
            Push("k", "v1", memorySvc);
            Assert.AreEqual("v1", Get("k", memorySvc));
            Push("k", "v2", memorySvc);
            Assert.AreEqual("v2", Get("k", memorySvc));
            Assert.Catch<ArgumentNullException>(() => memorySvc.Push("v", string.Empty, "c", "a"));
            Assert.Catch<ArgumentNullException>(() => memorySvc.Push("v", "k", string.Empty, "a"));
            Assert.Catch<ArgumentNullException>(() => memorySvc.Push("v", "k", "c", null));

            // due to allowing string.Empty as value of app, the following statement would not throwing exception
            memorySvc.Push("v", "k", "c", string.Empty);
        }

        [Test]
        public void Get()
        {
            var memorySvc = Svc;
            Push("k1", "v", memorySvc);
            Assert.NotNull(Get("k1", memorySvc));
            Assert.IsNull(Get("k2", memorySvc));
            Assert.Catch<ArgumentNullException>(() => memorySvc.Get(string.Empty, "c", "a"));
            Assert.Catch<ArgumentNullException>(() => memorySvc.Get("k", string.Empty, "a"));
            Assert.Catch<ArgumentNullException>(() => memorySvc.Get("k", "c", null));

            // due to allowing string.Empty as value of app, the following statement would not throwing exception
            memorySvc.Get("k", "c", string.Empty);
        }

        [Test]
        public void Clear()
        {
            var memorySvc = Svc;
            Push("k1", "v", memorySvc);
            Push("k2", "v", memorySvc);
            Push("k3", "v", memorySvc);

            Assert.Catch<ArgumentNullException>(() => memorySvc.Clear(null, "a"));
            Assert.Catch<ArgumentNullException>(() => memorySvc.Clear(string.Empty, "a"));
            Assert.Catch<ArgumentNullException>(() => memorySvc.Clear("c", null));
            memorySvc.Clear("c", string.Empty);
            Assert.NotNull(Get("k1", memorySvc));
            Assert.NotNull(Get("k2", memorySvc));
            Assert.NotNull(Get("k3", memorySvc));
            
            Clear(memorySvc);
            Assert.IsNull(Get("k1", memorySvc));
            Assert.IsNull(Get("k2", memorySvc));
            Assert.IsNull(Get("k3", memorySvc));
        }
    }
}