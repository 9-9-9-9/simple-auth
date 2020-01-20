using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;
using Test.Shared;

namespace Test.SimpleAuth.Shared.Services
{
    public class TestIRoleService : BaseTestClass
    {
        [TestCase(true, null, "c", "a", "e", "t", "m")]
        [TestCase(true, null, "c", "a", "e", "t", "m", "1")]
        [TestCase(true, null, "c", "a", "e", "t", "m", "1", "2")]
        [TestCase(false, typeof(EntityAlreadyExistsException), "c", "a", "e", "t", "m", "1", "2")]
        [TestCase(false, typeof(EntityAlreadyExistsException), "c", "a", "e", "t", "m", "1")]
        [TestCase(false, typeof(EntityAlreadyExistsException), "c", "a", "e", "t", "m")]
        public async Task AddRoleAsync(bool expectCreated, Type expectedException, string corp, string app, string env,
            string tenant, string module, params string[] subModules)
        {
            if (expectCreated && expectedException != null)
                Assert.Pass("Wrong test case");

            await Truncate<Role>(nameof(AddRoleAsync));

            var svc = Svc<IRoleService>();
            try
            {
                await AddRoleAsync(svc, corp, app, env, tenant, module, subModules);
                if (!expectCreated)
                {
                    Assert.Fail();
                }
            }
            catch (Exception e)
            {
                if (expectCreated)
                {
                    Assert.Fail();
                }
                else
                {
                    Assert.AreEqual(expectedException, e.GetType());
                }
            }
        }

        [Test]
        public async Task AddRoleAsync2()
        {
            await Truncate<Role>(nameof(AddRoleAsync2));

            var svc = Svc<IRoleService>();
            await AddRoleAsync(svc, "c", "a1", "e", "t", "m1");
            await AddRoleAsync(svc, "c", "a1", "e", "t", "m2");
            await AddRoleAsync(svc, "c", "a1", "e", "t", "m3");
            await AddRoleAsync(svc, "c", "a2", "e", "t", "m1");
            await AddRoleAsync(svc, "c", "a2", "e", "t", "m2");
            await AddRoleAsync(svc, "c", "a2", "e", "t", "m3");

            var repo = Svc<IRoleRepository>();
            Assert.AreEqual(3, repo.Search("e.t.m", "c", "a1").Count());
            Assert.AreEqual(3, repo.Search("e.t.m", "c", "a2").Count());
        }

        [TestCase(true, true, null, "c", "a", "e", "t", "m")]
        [TestCase(true, false, null, "c", "a", "e", "t", "m")]
        [TestCase(false, false, typeof(EntityNotExistsException), "c", "a2", "e", "t", "m")]
        public async Task UpdateLockStatus(bool expectedSuccess, bool create, Type expectedException, string corp, string app, string env,
            string tenant, string module)
        {
            await Truncate<Role>(nameof(UpdateLockStatus));

            var svc = Svc<IRoleService>();

            if (create)
                await AddRoleAsync(svc, corp, app, env, tenant, module);

            try
            {
                await svc.UpdateLockStatus(new global::SimpleAuth.Shared.Domains.Role()
                {
                    RoleId = $"{corp}.{app}.{env}.{tenant}.{module}",
                    Locked = true
                });
                if (!expectedSuccess)
                {
                    Assert.Fail();
                }
            }
            catch (Exception e)
            {
                if (expectedSuccess)
                {
                    Assert.Fail();
                }
                else
                {
                    Assert.AreEqual(expectedException, e.GetType());
                }
            }
        }

        [Test]
        public async Task SearchRole()
        {
            var randomCorp = Guid.NewGuid().ToString()
                .Replace("a", "")
                .Replace("e", "")
                .Replace("1", "")
                .Replace("2", "")
                .Replace("3", "")
                .Replace("4", "")
                .Replace("-", "")
                .Substring(5);
            var svc = Svc<IRoleService>();

            await AddRoleAsync(svc, randomCorp, "a", "e1", "t", "m");
            await AddRoleAsync(svc, randomCorp, "a", "e", "t1", "m");
            await AddRoleAsync(svc, randomCorp, "a", "e", "t", "m1");
            await AddRoleAsync(svc, randomCorp, "a", "e", "t2", "m");
            await AddRoleAsync(svc, randomCorp, "a", "e2", "t1", "m");
            await AddRoleAsync(svc, randomCorp, "a", "e2", "t1", "*");
            await AddRoleAsync(svc, randomCorp, "a", "e3", "*", "m3");
            await AddRoleAsync(svc, randomCorp, "a", "e3", "t3", "*");
            
            Assert.AreEqual(5, svc.SearchRoles("1", randomCorp, "a").Select(x => x.RoleId).Count());
            Assert.AreEqual(3, svc.SearchRoles("t1", randomCorp, "a").Select(x => x.RoleId).Count());
            Assert.AreEqual(3, svc.SearchRoles("2", randomCorp, "a").Select(x => x.RoleId).Count());
            Assert.AreEqual(8, svc.SearchRoles("e", randomCorp, "a").Select(x => x.RoleId).Count());
            Assert.AreEqual(3, svc.SearchRoles("*", randomCorp, "a").Select(x => x.RoleId).Count());
            Assert.AreEqual(1, svc.SearchRoles("*.", randomCorp, "a").Select(x => x.RoleId).Count());
            Assert.AreEqual(3, svc.SearchRoles(".*", randomCorp, "a").Select(x => x.RoleId).Count());
            Assert.AreEqual(0, svc.SearchRoles("4", randomCorp, "a").Select(x => x.RoleId).Count());
            
            await AddRoleAsync(svc, randomCorp, "aa", "e", "t", "m", "s1", "s2");
            await AddRoleAsync(svc, randomCorp, "aa", "e", "t", "m", "s1", "s3");
            await AddRoleAsync(svc, randomCorp, "aa", "e", "t", "s3", "s1", "s4");
            Assert.AreEqual(3, svc.SearchRoles("s1|", randomCorp, "aa").Select(x => x.RoleId).Count());
            Assert.AreEqual(1, svc.SearchRoles("s1|s2", randomCorp, "aa").Select(x => x.RoleId).Count());
            Assert.AreEqual(1, svc.SearchRoles("s1|s3", randomCorp, "aa").Select(x => x.RoleId).Count());
            Assert.AreEqual(1, svc.SearchRoles("|s3", randomCorp, "aa").Select(x => x.RoleId).Count());
            Assert.AreEqual(2, svc.SearchRoles("s3", randomCorp, "aa").Select(x => x.RoleId).Count());
        }
    }
}