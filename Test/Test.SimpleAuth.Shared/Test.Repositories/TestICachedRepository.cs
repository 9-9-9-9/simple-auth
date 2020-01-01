using System;
using NUnit.Framework;
using Test.SimpleAuth.Shared.Mock.Repositories;

namespace Test.SimpleAuth.Shared.Test.Repositories
{
    public class TestICachedRepository : BaseTestClass
    {
        [Test]
        public void MemoryCachedRepository()
        {
            var svc = Svc<IDummyMemoryCachedRepository>();
            Assert.IsNull(svc.Get("k", "c", "a"));
            svc.Push("v", "k", "c", "a");
            Assert.AreEqual("v", svc.Get("k", "c", "a"));

            Assert.IsNull(svc.Get("k", "c", "a1"));
            Assert.IsNull(svc.Get("k", "c1", "a"));
            Assert.IsNull(svc.Get("k1", "c", "a"));

            svc.Clear("c", "a");
            Assert.IsNull(svc.Get("k", "c", "a"));

            Assert.That(() => svc.Clear(null, "a"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => svc.Clear("c", null), Throws.TypeOf<ArgumentNullException>());

            Assert.That(() => svc.Push("v", null, "c", "a"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => svc.Push("v", "k", null, "a"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => svc.Push("v", "k", "c", null), Throws.TypeOf<ArgumentNullException>());

            Assert.That(() => svc.Get(null, "c", "a"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => svc.Get("k", null, "a"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => svc.Get("k", "c", null), Throws.TypeOf<ArgumentNullException>());

            svc.Push(null, "k2", "c", "a");
            Assert.IsNull(svc.Get("k2", "c", "a"));
        }
    }
}