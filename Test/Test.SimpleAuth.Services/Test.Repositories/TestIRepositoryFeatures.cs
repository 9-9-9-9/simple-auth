using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using Test.Shared;

namespace Test.SimpleAuth.Shared.Test.Repositories
{
    public class TestIRepositoryFeatures : BaseTestClass
    {
        [Test]
        public async Task FindOrdered()
        {
            var svc = Svc<IRoleService>();
            var repo = Svc<IRoleRepository>();

            var corp = RandomCorp();
            await AddRoleAsync(svc, corp, "a", "e", "t", "m1");
            await AddRoleAsync(svc, corp, "a", "e", "t", "m3");
            await AddRoleAsync(svc, corp, "a", "e", "t", "m4");

            var findOrdered = repo.FindOrdered(x => x.Corp == corp, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Asc,
                Expression = x => x.Module
            }, findOptions: new FindOptions {Skip = 1}).ToList();

            Assert.AreEqual(2, findOrdered.Count);
            Assert.AreEqual("m3", findOrdered.First().Module);
            Assert.AreEqual("m4", findOrdered.Skip(1).First().Module);

            findOrdered = repo.FindOrdered(x => x.Corp == corp, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Desc,
                Expression = x => x.Module
            }, findOptions: new FindOptions {Skip = 1}).ToList();

            Assert.AreEqual(2, findOrdered.Count);
            Assert.AreEqual("m3", findOrdered.First().Module);
            Assert.AreEqual("m1", findOrdered.Skip(1).First().Module);

            findOrdered = repo.FindOrdered(x => x.Corp == corp, orderByOption: new OrderByOptions<Role, string>
            {
                Direction = OrderDirection.Default,
                Expression = x => x.Module
            }).ToList();

            Assert.AreEqual(3, findOrdered.Count);
            Assert.AreEqual("m1", findOrdered.First().Module);
            Assert.AreEqual("m3", findOrdered.Skip(1).First().Module);
            Assert.AreEqual("m4", findOrdered.Skip(2).First().Module);
        }

        [Test]
        public async Task WithFindOptions()
        {
            var svc = Svc<IRoleService>();
            var repo = Svc<IRoleRepository>();

            var corp = RandomCorp();
            var app = RandomCorp();
            await AddRoleAsync(svc, corp, app, "e", "t", "m1");
            await AddRoleAsync(svc, corp, app, "e", "t", "m2");
            await AddRoleAsync(svc, corp, app, "e", "t", "m3");

            var results = Find(0, 0);
            Assert.AreEqual(3, results.Length);

            results = Find(1, 0);
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("m2", results[0].Module);
            //
            results = FindOrdered(skip: 2);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("m3", results[0].Module);
            //
            results = Find(0, 1);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("m1", results[0].Module);
            //
            results = Find(0, 2);
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("m1", results[0].Module);
            Assert.AreEqual("m2", results[1].Module);
            //
            results = Find(1, 1);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("m2", results[0].Module);
            //
            results = Find(2, 1);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("m3", results[0].Module);
            //
            results = Find(999, 999);
            Assert.AreEqual(0, results.Length);

            Role[] Find(int skip, int take)
            {
                return repo.Find(x => x.Corp == corp && x.App == app, new FindOptions
                {
                    Skip = skip,
                    Take = take
                }).ToArray();
            }

            Role[] FindOrdered(int skip = 0, int take = 0, OrderDirection orderDirection = OrderDirection.Default)
            {
                return repo.FindOrdered(x => x.Corp == corp && x.App == app, new FindOptions
                {
                    Skip = skip,
                    Take = take
                }, new OrderByOptions<Role, string>
                {
                    Expression = x => x.Module,
                    Direction = orderDirection
                }).ToArray();
            }
        }
    }
}