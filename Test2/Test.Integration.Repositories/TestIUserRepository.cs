using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;

namespace Test.Integration.Repositories
{
    public class TestIUserRepository : BaseTestRepo
    {
        [Test]
        public async Task CreateUserAsync()
        {
            var repo = Svc<IUserRepository>();

            var userId = RandomUser();
            var corp = RandomCorp();
            var user = new User
            {
                Id = userId,
                NormalizedId = userId,
            };
            var userInfo = new LocalUserInfo
            {
                UserId = userId,
                Corp = corp,
            };

            // expect success
            await Create();

            // user already at corp
            Assert.CatchAsync<EntityAlreadyExistsException>(async () => await Create());
            
            // expect success
            await Create(new LocalUserInfo
            {
                UserId = userId,
                Corp = RandomCorp()
            });

            Assert.AreEqual(2, repo.Find(userId).UserInfos.Count);

            Task Create(LocalUserInfo userInfoCustom = null)
            {
                return repo.CreateUserAsync(user, userInfoCustom ?? userInfo);
            }
        }
    }
}