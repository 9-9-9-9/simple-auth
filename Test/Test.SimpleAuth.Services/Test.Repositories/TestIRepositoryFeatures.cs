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
            await AddRoleAsync(svc, corp, "a", "e", "t", "m1");
            await AddRoleAsync(svc, corp, "a", "e", "t", "m2");
            await AddRoleAsync(svc, corp, "a", "e", "t", "m3");

            var results = repo.Find(x => x.Corp == corp).ToList();
            Assert.AreEqual(3, results.Count);

            results = repo.Find(x => x.Corp == corp, new FindOptions
            {
                Skip = 1
            }).ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("m2", results.Skip(0).First().Module);
//
            results = repo.Find(x => x.Corp == corp, new FindOptions
            {
                Skip = 2
            }).ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("m3", results.Skip(0).First().Module);
//
            results = repo.Find(x => x.Corp == corp, new FindOptions
            {
                Take = 1
            }).ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("m1", results.Skip(0).First().Module);
//
            results = repo.Find(x => x.Corp == corp, new FindOptions
            {
                Take = 2
            }).ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("m1", results.Skip(0).First().Module);
            Assert.AreEqual("m2", results.Skip(1).First().Module);
//
            results = repo.Find(x => x.Corp == corp, new FindOptions
            {
                Skip = 1,
                Take = 1
            }).ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("m2", results.Skip(0).First().Module);
//
            results = repo.Find(x => x.Corp == corp, new FindOptions
            {
                Skip = 2,
                Take = 1
            }).ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("m3", results.Skip(0).First().Module);
//
            results = repo.Find(x => x.Corp == corp, new FindOptions
            {
                Skip = 999,
                Take = 999
            }).ToList();
            Assert.AreEqual(0, results.Count);
        }
    }
}