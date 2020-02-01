using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;
using Test.Shared;
using Test.Shared.Utils;

namespace Test.SimpleAuth.Services.Test.Services
{
    public class TestITokenInfoService : BaseTestClass
    {
        protected IServiceProvider Prepare(out Mock<ITokenInfoRepository> mockTokenInfoRepository)
        {
            mockTokenInfoRepository = Mu.Of<ITokenInfoRepository>();
            var repoObj = mockTokenInfoRepository.Object;
            return Prepare(services => { services.AddSingleton(repoObj); });
        }

        [Test]
        public async Task IncreaseVersionAsync()
        {
            var svc = Prepare(out var mockTokenInfoRepository).GetRequiredService<ITokenInfoService>();

            mockTokenInfoRepository.Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<TokenInfo>>())).ReturnsAsync(1);
            mockTokenInfoRepository.Setup(x => x.UpdateManyAsync(It.IsAny<IEnumerable<TokenInfo>>())).ReturnsAsync(1);

            mockTokenInfoRepository
                .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<TokenInfo, bool>>>>()))
                .ReturnsAsync(new TokenInfo
                {
                    Corp = "c",
                    App = "a",
                    Version = 1
                });

            Assert.AreEqual(2, await svc.IncreaseVersionAsync(new global::SimpleAuth.Shared.Domains.TokenInfo
            {
                Corp = "c",
                App = "a",
            }));

            // ReSharper disable PossibleMultipleEnumeration
            mockTokenInfoRepository.Verify(m =>
                m.UpdateManyAsync(It.Is<IEnumerable<TokenInfo>>(args =>
                    args.First().Version == 2 && args.First().Corp == "c" && args.First().App == "a")));

            mockTokenInfoRepository
                .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<TokenInfo, bool>>>>()))
                .ReturnsAsync((TokenInfo) null);

            Assert.AreEqual(1, await svc.IncreaseVersionAsync(new global::SimpleAuth.Shared.Domains.TokenInfo
            {
                Corp = "c2",
                App = "a2",
            }));

            mockTokenInfoRepository.Verify(m =>
                m.CreateManyAsync(It.Is<IEnumerable<TokenInfo>>(args =>
                    args.First().Version == 1 && args.First().Corp == "c2" && args.First().App == "a2" &&
                    args.First().Id != Guid.Empty)));
            // ReSharper restore PossibleMultipleEnumeration

            Assert.AreEqual(2, await svc.GetCurrentVersionAsync(new global::SimpleAuth.Shared.Domains.TokenInfo
            {
                Corp = "c",
                App = "a",
                // ReSharper disable once ArgumentsStyleLiteral
                // ReSharper disable once RedundantArgumentDefaultValue
            }, noCaching: false));

            Assert.AreEqual(1, await svc.GetCurrentVersionAsync(new global::SimpleAuth.Shared.Domains.TokenInfo
            {
                Corp = "c2",
                App = "a2",
                // ReSharper disable once ArgumentsStyleLiteral
                // ReSharper disable once RedundantArgumentDefaultValue
            }, noCaching: false));
        }

        [Test]
        public async Task GetCurrentVersionAsync()
        {
            var svc = Prepare(out var mockTokenInfoRepository).GetRequiredService<ITokenInfoService>();

            WhenFind()
                .ReturnsAsync(new TokenInfo
                {
                    Corp = "c",
                    App = "a",
                    Version = 1
                });

            Assert.AreEqual(1, await GetUnderCaching());

            WhenFind()
                .ReturnsAsync(new TokenInfo
                {
                    Corp = "c",
                    App = "a",
                    Version = 999
                });

            Assert.AreEqual(1, await GetUnderCaching());

            Assert.AreEqual(999, await GetWithoutCaching());

            WhenFind()
                .ReturnsAsync((TokenInfo)null);

            Assert.AreEqual(0, await GetWithoutCaching());

            WhenFind()
                .ThrowsAsync(new EntityAlreadyExistsException("a"));

            Assert.CatchAsync<EntityAlreadyExistsException>(async () => await GetWithoutCaching());

            Task<int> GetWithoutCaching() => svc.GetCurrentVersionAsync(new global::SimpleAuth.Shared.Domains.TokenInfo
            {
                Corp = "c",
                App = "a",
                // ReSharper disable once ArgumentsStyleLiteral
            }, noCaching: true);

            Task<int> GetUnderCaching() => svc.GetCurrentVersionAsync(new global::SimpleAuth.Shared.Domains.TokenInfo
            {
                Corp = "c",
                App = "a",
                // ReSharper disable once ArgumentsStyleLiteral
                // ReSharper disable once RedundantArgumentDefaultValue
            }, noCaching: false);

            ISetup<ITokenInfoRepository, Task<TokenInfo>> WhenFind() => mockTokenInfoRepository
                .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<TokenInfo, bool>>>>()));
        }
    }
}