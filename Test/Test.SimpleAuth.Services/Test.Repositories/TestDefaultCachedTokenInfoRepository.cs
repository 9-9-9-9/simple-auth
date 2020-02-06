using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;

namespace Test.SimpleAuth.Shared.Test.Repositories
{
    public class TestDefaultCachedTokenInfoRepository
    {
        /*
        [Test]
        public void AllInOne()
        {
            ICachedTokenInfoRepository repo = new CachedTokenInfoRepository();
            repo.Push(new TokenInfo
            {
                Corp = "c",
                App = "a",
                Version = 3
            });

            var tokenInfo = repo.Get("c", "a");
            Assert.AreEqual("c", tokenInfo.Corp);
            Assert.AreEqual("a", tokenInfo.App);
            Assert.AreEqual(3, tokenInfo.Version);
            
            repo.Push(new TokenInfo
            {
                Corp = "c",
                App = "a",
                Version = 5
            });
            
            tokenInfo = repo.Get("c", "a");
            Assert.AreEqual("c", tokenInfo.Corp);
            Assert.AreEqual("a", tokenInfo.App);
            Assert.AreEqual(5, tokenInfo.Version);
            
            repo.Push(new TokenInfo
            {
                Corp = "d",
                App = "b",
                Version = 9
            });
            
            tokenInfo = repo.Get("c", "a");
            Assert.AreEqual("c", tokenInfo.Corp);
            Assert.AreEqual("a", tokenInfo.App);
            Assert.AreEqual(5, tokenInfo.Version);
            
            tokenInfo = repo.Get("d", "b");
            Assert.AreEqual("d", tokenInfo.Corp);
            Assert.AreEqual("b", tokenInfo.App);
            Assert.AreEqual(9, tokenInfo.Version);
            
            repo.Clear("c", "a");
            Assert.IsNull(repo.Get("c", "a"));
            Assert.IsNotNull(repo.Get("d", "b"));
        }
        */
    }
}