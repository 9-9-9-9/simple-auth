using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Utils;
using Test.Shared;

namespace Test.Integration.Repositories
{
    public class TestIRepository : BaseTestRepo
    {
        [Test]
        public async Task CreateManyAsync()
        {
            var (repo, corp) = await GenerateRolesAsync();
            var roles = repo.Find(x => x.Corp == corp);
            Assert.AreEqual(6, roles.Count());
        }

        [Test]
        public async Task FindSingleAsync()
        {
            var (repo, corp) = await GenerateRolesAsync();

            // expect err: Sequence contains more than one element
            Assert.CatchAsync<InvalidOperationException>(async () =>
                await repo.FindSingleAsync(x => x.Corp == corp && x.Module == "m1"));

            // expect success when result is unique
            await repo.FindSingleAsync(x => x.Corp == corp && x.Module == "m1" && x.App == "a1");

            // expect no error when no result
            Assert.IsNull(await repo.FindSingleAsync(x => x.Corp == RandomCorp()));
        }

        [Test]
        public async Task FindMany_Without_FindOptions()
        {
            var (repo, corp) = await GenerateRolesAsync();

            var roles = repo.FindMany(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            });

            Assert.AreEqual(2, roles.Count());

            Expression<Func<Role, bool>> Exp(Expression<Func<Role, bool>> exp) => exp;
        }

        [Test]
        public async Task FindMany_With_FindOptions()
        {
            var (repo, corp) = await GenerateRolesAsync();

            var roles = repo.FindMany(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, new FindOptions
            {
                Skip = 1
            });

            Assert.AreEqual(1, roles.Count());

            roles = repo.FindMany(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, new FindOptions
            {
                Take = 1
            });

            Assert.AreEqual(1, roles.Count());

            roles = repo.FindMany(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, new FindOptions
            {
                Skip = 1,
                Take = 999
            });

            Assert.AreEqual(1, roles.Count());

            Expression<Func<Role, bool>> Exp(Expression<Func<Role, bool>> exp) => exp;
        }

        [Test]
        public async Task FindManyOrdered_Without_FindOptions()
        {
            var (repo, corp) = await GenerateRolesAsync();

            // default sorting
            var roles = repo.FindManyOrdered(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Default,
                Expression = x => x.Id
            }).OrEmpty().ToList();

            Assert.AreEqual(2, roles.Count);
            Assert.AreEqual("a1", roles.Skip(0).First().App);
            Assert.AreEqual("a2", roles.Skip(1).First().App);

            // sort by ASC
            roles = repo.FindManyOrdered(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Asc,
                Expression = x => x.Id
            }).OrEmpty().ToList();

            Assert.AreEqual(2, roles.Count);
            Assert.AreEqual("a1", roles.Skip(0).First().App);
            Assert.AreEqual("a2", roles.Skip(1).First().App);

            // sort by DESC
            roles = repo.FindManyOrdered(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Desc,
                Expression = x => x.Id
            }).OrEmpty().ToList();

            Assert.AreEqual(2, roles.Count);
            Assert.AreEqual("a2", roles.Skip(0).First().App);
            Assert.AreEqual("a1", roles.Skip(1).First().App);


            Expression<Func<Role, bool>> Exp(Expression<Func<Role, bool>> exp) => exp;
        }

        [Test]
        public async Task FindManyOrdered_With_FindOptions()
        {
            var (repo, corp) = await GenerateRolesAsync();

            // default sorting
            var roles = repo.FindManyOrdered(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Default,
                Expression = x => x.Id
            }, findOptions: new FindOptions
            {
                Skip = 1
            }).OrEmpty().ToList();

            Assert.AreEqual(1, roles.Count);
            Assert.AreEqual("a2", roles.Skip(0).First().App);

            // sort by ASC
            roles = repo.FindManyOrdered(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Asc,
                Expression = x => x.Id
            }, findOptions: new FindOptions
            {
                Skip = 1
            }).OrEmpty().ToList();

            Assert.AreEqual(1, roles.Count);
            Assert.AreEqual("a2", roles.Skip(0).First().App);
            //
            roles = repo.FindManyOrdered(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Asc,
                Expression = x => x.Id
            }, findOptions: new FindOptions
            {
                Skip = 1,
                Take = 4
            }).OrEmpty().ToList();

            Assert.AreEqual(1, roles.Count);
            Assert.AreEqual("a2", roles.Skip(0).First().App);

            // sort by DESC
            roles = repo.FindManyOrdered(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Desc,
                Expression = x => x.Id
            }, findOptions: new FindOptions
            {
                Skip = 1
            }).OrEmpty().ToList();

            Assert.AreEqual(1, roles.Count);
            Assert.AreEqual("a1", roles.Skip(0).First().App);
            //
            roles = repo.FindManyOrdered(new[]
            {
                Exp(x => x.Corp == corp && x.Module == "m1")
            }, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Desc,
                Expression = x => x.Id
            }, findOptions: new FindOptions
            {
                Skip = 1,
                Take = 1
            }).OrEmpty().ToList();

            Assert.AreEqual(1, roles.Count);
            Assert.AreEqual("a1", roles.Skip(0).First().App);


            Expression<Func<Role, bool>> Exp(Expression<Func<Role, bool>> exp) => exp;
        }

        [Test]
        public async Task UpdateManyAsync()
        {
            var (repo, corp) = await GenerateRolesAsync();
            var roles = repo.Find(x => x.Corp == corp).ToList();
            roles.ForEach(x => x.Corp = RandomCorp());
            await repo.UpdateManyAsync(roles);

            Assert.IsTrue(repo.Find(x => x.Corp == corp).IsEmpty());
        }

        [Test]
        public async Task DeleteManyAsync()
        {
            var (repo, corp) = await GenerateTokensAsync();

            var roles = repo.Find(x => x.Corp == corp).ToList();

            await repo.DeleteManyAsync(roles.Where(x => x.App == "a1"));

            Assert.AreEqual(2, repo.Find(x => x.Corp == corp).Count());
        }

        [Test]
        public async Task TruncateTable()
        {
            var (repo, corp) = await GenerateRolesAsync();
            Assert.AreEqual(6, repo.Find(x => x.Corp == corp).Count());
            await repo.TruncateTable();
            Assert.IsTrue(repo.Find(x => x.Corp == corp).IsEmpty());
        }

        [Test]
        public async Task Find_ById()
        {
            var (repo, corp) = await GenerateRolesAsync();
            Assert.NotNull(repo.Find($"{corp}.a1.e.t.m1"));
            Assert.NotNull(await repo.FindAsync($"{corp}.a1.e.t.m1"));
        }

        [Test]
        public async Task ExtensionsOfIRepository()
        {
            var (roleRepo, corp1) = await GenerateRolesAsync();
            var (tokenRepo, corp2) = await GenerateTokensAsync();

            //CreateAsync
            await roleRepo.CreateAsync(new Role
            {
                Corp = corp1,
                App = "a3",
                Env = "e",
                Tenant = "t",
                Module = "m1",
                SubModules = RoleUtils.JoinSubModules(new[] {"s", "1"})
            }.ComputeId());
            Assert.NotNull(roleRepo.Find($"{corp1}.a3.e.t.m1.s|1"));

            //FindOrdered
            var roles = roleRepo.FindOrdered(x => x.Corp == corp1,
                orderByOption: new OrderByOptions<Role, string>
                {
                    Direction = OrderDirection.Desc,
                    Expression = x => x.Id
                }).ToList();
            Assert.AreEqual(6 + 1 /*new added a3*/, roles.Count);

            //FindSingleAsync
            Assert.CatchAsync<InvalidOperationException>(async () =>
                await roleRepo.FindSingleAsync(x => x.Corp == corp1));

            //Find
            Assert.AreEqual(6 + 1 /*new added a3*/, roleRepo.Find(x => x.Corp == corp1).Count());

            //UpdateAsync
            var role = await roleRepo.FindSingleAsync(x => x.Id == $"{corp1}.a3.e.t.m1.s|1");
            role.Corp = corp2;
            await roleRepo.UpdateAsync(role);
            Assert.AreEqual(corp2, (await roleRepo.FindSingleAsync(x => x.Id == $"{corp1}.a3.e.t.m1.s|1")).Corp);

            //DeleteAsync
            var rg = await tokenRepo.FindSingleAsync(x => x.Corp == corp2 && x.App == "a2");
            await tokenRepo.DeleteAsync(rg);
            Assert.IsNull(await tokenRepo.FindSingleAsync(x => x.Corp == corp2 && x.App == "a2"));
        }
    }

    public class TestOtherRepos : BaseTestClass
    {
        [Test]
        public async Task LocalUserInfo()
        {
            var sp = Prepare();
            var userRepo = sp.GetRequiredService<IUserRepository>();
            var localUserInfoRepo = sp.GetRequiredService<ILocalUserInfoRepository>();
            var userId = RandomUser();
            var corp = RandomCorp();

            await userRepo.CreateUserAsync(new User
            {
                Id = userId,
            }, new LocalUserInfo
            {
                UserId = userId,
                Corp = corp
            });

            var user = userRepo.Find(userId);
            Assert.NotNull(user);
            Assert.AreEqual(1, user.UserInfos.Count);

            var localUserInfo = localUserInfoRepo.Find(x => x.UserId == userId).ToList();
            Assert.NotNull(localUserInfo);
            Assert.AreEqual(1, localUserInfo.Count);
        }

        [Test]
        public async Task RoleGroupUser()
        {
            var sp = Prepare();
            var userRepo = sp.GetRequiredService<IUserRepository>();
            var groupRepo = sp.GetRequiredService<IRoleGroupRepository>();
            var roleGroupUserRepo = sp.GetRequiredService<IRoleGroupUserRepository>();
            var userId = RandomUser();
            var corp = RandomCorp();
            var app = RandomApp();
            var groupId = RandomRoleGroup();

            await userRepo.CreateUserAsync(new User
            {
                Id = userId,
            }, new LocalUserInfo
            {
                UserId = userId,
                Corp = corp
            });

            var user = userRepo.Find(userId);
            await groupRepo.CreateAsync(new RoleGroup
            {
                Name = groupId,
                Corp = corp,
                App = app,
            });

            await userRepo.AssignUserToGroups(user, groupRepo.Find(x => x.Name == groupId).ToArray());
            
            // Verify
            user = userRepo.Find(userId);
            Assert.AreEqual(1, user.RoleGroupUsers.Count);
            var roleGroupUsers = roleGroupUserRepo.Find(x => x.UserId == userId).ToList();
            Assert.AreEqual(1, roleGroupUsers.Count);
        }
    }
}