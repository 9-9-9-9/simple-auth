using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Utils;
using Test.Shared;

namespace Test.Integration.Repositories
{
    public class TestIRepository : BaseTestClass
    {
        [Test]
        public async Task CreateManyAsync()
        {
            var (repo, corp) = await GenerateRecordsAsync();
            var roles = repo.Find(x => x.Corp == corp);
            Assert.AreEqual(6, roles.Count());
        }

        [Test]
        public async Task FindSingleAsync()
        {
            var (repo, corp) = await GenerateRecordsAsync();

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
            var (repo, corp) = await GenerateRecordsAsync();

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
            var (repo, corp) = await GenerateRecordsAsync();

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
            var (repo, corp) = await GenerateRecordsAsync();

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
            var (repo, corp) = await GenerateRecordsAsync();

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
            var (repo, corp) = await GenerateRecordsAsync();
            var roles = repo.Find(x => x.Corp == corp).ToList();
            roles.ForEach(x => x.Corp = RandomCorp());
            await repo.UpdateManyAsync(roles);

            Assert.IsTrue(repo.Find(x => x.Corp == corp).IsEmpty());
        }

        [Test]
        public async Task DeleteManyAsync()
        {
            var (repo, corp) = await GenerateGroupRecordsAsync();

            var roles = repo.Find(x => x.Corp == corp).ToList();

            await repo.DeleteManyAsync(roles.Where(x => x.App == "a1"));

            Assert.AreEqual(1, repo.Find(x => x.Corp == corp).Count());
        }

        [Test]
        public async Task TruncateTable()
        {
            var (repo, corp) = await GenerateRecordsAsync();
            Assert.AreEqual(6, repo.Find(x => x.Corp == corp).Count());
            await repo.TruncateTable();
            Assert.IsTrue(repo.Find(x => x.Corp == corp).IsEmpty());
        }

        [Test]
        public async Task Find_ById()
        {
            var (repo, corp) = await GenerateRecordsAsync();
            Assert.NotNull(repo.Find($"{corp}.a1.e.t.m1"));
            Assert.NotNull(await repo.FindAsync($"{corp}.a1.e.t.m1"));
        }

        [Test]
        public async Task ExtensionsOfIRepository()
        {
            var (roleRepo, corp1) = await GenerateRecordsAsync();
            var (roleGroupRepo, corp2) = await GenerateGroupRecordsAsync();

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
            Assert.AreEqual(2, roleRepo.FindOrdered(x => x.App == "a1", orderByOption: new OrderByOptions<Role,string>
            {
                Direction = OrderDirection.Desc,
                Expression = x => x.Id
            }).Count());
            
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
            var rg = await roleGroupRepo.FindSingleAsync(x => x.Corp == corp2 && x.Name == "g1");
            await roleGroupRepo.DeleteAsync(rg);
            Assert.IsNull(await roleGroupRepo.FindSingleAsync(x => x.Corp == corp2 && x.Name == "g1"));
        }

        private async Task<(IRoleRepository, string)> GenerateRecordsAsync()
        {
            var roleRepository = Svc<IRoleRepository>();
            var corp = RandomCorp();

            await roleRepository.CreateManyAsync(new List<Role>
            {
                new Role
                {
                    Corp = corp,
                    App = "a1",
                    Env = "e",
                    Tenant = "t",
                    Module = "m1"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a1",
                    Env = "e",
                    Tenant = "t",
                    Module = "m2"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a2",
                    Env = "e",
                    Tenant = "t",
                    Module = "m1"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a2",
                    Env = "e",
                    Tenant = "t",
                    Module = "m2"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a2",
                    Env = "e",
                    Tenant = "t",
                    Module = "m3"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a2",
                    Env = "e",
                    Tenant = "t",
                    Module = "m4"
                }.ComputeId(),
            });

            return (roleRepository, corp);
        }

        private async Task<(IRoleGroupRepository, string)> GenerateGroupRecordsAsync()
        {
            var roleGroupRepository = Svc<IRoleGroupRepository>();
            var corp = RandomCorp();

            await roleGroupRepository.CreateManyAsync(new[]
            {
                new RoleGroup
                {
                    Corp = corp,
                    App = "a1",
                    Name = "g1",
                }.WithRandomId(),
                new RoleGroup
                {
                    Corp = corp,
                    App = "a2",
                    Name = "g2"
                }.WithRandomId()
            });

            return (roleGroupRepository, corp);
        }
    }
}