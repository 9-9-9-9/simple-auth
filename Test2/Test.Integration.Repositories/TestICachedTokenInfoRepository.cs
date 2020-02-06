using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using Test.Shared;

namespace Test.Integration.Repositories
{
    public class TestICachedTokenInfoRepository : BaseTestClass
    {
        [Test]
        public void VerifyCorpAndAppAlwaysRequired()
        {
            var repo = Svc<ICachedTokenInfoRepository>();
            // PushAsync
            Assert.CatchAsync<ArgumentNullException>(async () => await repo.PushAsync(null));
            Assert.CatchAsync<ArgumentException>(async () => await repo.PushAsync(new TokenInfo
            {
                Corp = string.Empty,
                App = "a",
            }));
            Assert.CatchAsync<ArgumentException>(async () => await repo.PushAsync(new TokenInfo
            {
                Corp = "c",
                App = null,
            }));
            // GetAsync
            Assert.CatchAsync<ArgumentNullException>(async () => await repo.GetAsync(string.Empty, "a"));
            Assert.CatchAsync<ArgumentNullException>(async () => await repo.GetAsync("c", null));
            // ClearAsync
            Assert.CatchAsync<ArgumentNullException>(async () => await repo.ClearAsync(string.Empty, "a"));
            Assert.CatchAsync<ArgumentNullException>(async () => await repo.ClearAsync("c", null));
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task FullTestRepo(string app)
        {
            var repo = Svc<ICachedTokenInfoRepository>();
            var corp = RandomCorp();
            app ??= RandomApp();
            var app2 = RandomApp();
            await repo.PushAsync(new TokenInfo
            {
                Corp = corp,
                App = app,
                Version = 2,
            });
            var token = await repo.GetAsync(corp, app);
            Assert.NotNull(token);
            Assert.AreEqual(2, token.Version);
            // update
            await repo.PushAsync(new TokenInfo
            {
                Corp = corp,
                App = app,
                Version = 3,
            });
            token = await repo.GetAsync(corp, app);
            Assert.AreEqual(3, token.Version);
            // add another
            await repo.PushAsync(new TokenInfo
            {
                Corp = corp,
                App = app2,
                Version = 5,
            });
            // clear
            await repo.ClearAsync(corp, app);
            token = await repo.GetAsync(corp, app);
            Assert.IsNull(token);
            //
            token = await repo.GetAsync(corp, app2);
            Assert.AreEqual(5, token.Version);
        }
    }
}