using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using Test.Shared;
using Test.Shared.Extensions;
using Test.Shared.Utils;
using Test.SimpleAuth.Server.Support.Extensions;

namespace Test.SimpleAuth.Services.Test.Services
{
    public class TestDefaultTokenInfoService : BaseTestClass
    {
        [Test]
        public async Task IncreaseVersionAsync()
        {
            var mockServiceProvider = Mu.OfServiceProviderFor<ITokenInfoService>()
                .With<ITokenInfoRepository>(out var mockRepo)
                .With<ICachedTokenInfoRepository>(out var mockCacheRepo, MockBehavior.Loose);

            mockRepo.SetupFindSingleAsync<ITokenInfoRepository, TokenInfo, Guid>(null);
            mockRepo.SetupCreateManyAsync<ITokenInfoRepository, TokenInfo, Guid>(1);
            mockRepo.SetupUpdateManyAsync<ITokenInfoRepository, TokenInfo, Guid>(1);

            ITokenInfoService defaultTokenInfoService =
                new DefaultTokenInfoService(mockServiceProvider.Object, mockCacheRepo.Object);

            var tokenInfo = new global::SimpleAuth.Shared.Domains.TokenInfo
            {
                Corp = "c",
                App = "a"
            };
            Assert.AreEqual(1, await defaultTokenInfoService.IncreaseVersionAsync(tokenInfo));

            
            mockRepo.SetupFindSingleAsync<ITokenInfoRepository, TokenInfo, Guid>(new TokenInfo
            {
                Version = 2
            });
            Assert.AreEqual(3, await defaultTokenInfoService.IncreaseVersionAsync(tokenInfo));
        }
    }
}