using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace Test.SimpleAuth.Services.Test.Objects
{
    public class TestBaseEntityExtensions
    {
        [Test]
        public void WithRandomId()
        {
            var gr = new RoleGroup();
            Assert.IsTrue(Guid.Empty.Equals(gr.Id));
            gr.WithRandomId();
            Assert.IsFalse(Guid.Empty.Equals(gr.Id));
            Assert.Catch<InvalidOperationException>(() => gr.WithRandomId());
        }
    }

    public class TestLocalUserInfo
    {
        [Test]
        public void ToDomainObject()
        {
            var eLui = new LocalUserInfo
            {
                UserId = "hello",
                Email = "test@simple.auth",
                NormalizedEmail = "test@simple.auth",
                Corp = "c",
                EncryptedPassword = "this-is-encrypted-password",
                Locked = true
            };

            var mLui = eLui.ToDomainObject();
            Assert.NotNull(mLui);
            Assert.AreEqual(eLui.Email, mLui.Email);
            Assert.AreEqual(eLui.Corp, mLui.Corp);
            Assert.AreEqual(eLui.Locked, mLui.Locked);
            Assert.IsNull(mLui.PlainPassword);
        }
    }

    public class TestRole
    {
        [TestCase("c", "a", "e", "t", "m", null, ExpectedResult = "c.a.e.t.m")]
        [TestCase("c", "a", "e", "t", "m", "s", ExpectedResult = "c.a.e.t.m.s")]
        [TestCase("c", "a", "e", "t", "m", "s|2", ExpectedResult = "c.a.e.t.m.s|2")]
        public string ComputeId(string corp, string app, string env, string tenant, string module, string subModule)
        {
            return new Role
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModule,
                Locked = true
            }.ComputeId().Id;
        }

        [Test]
        public void ComputeId_Err()
        {
            Assert.Catch<ArgumentNullException>(() => new Role
            {
                Corp = "c",
                App = "a",
                Env = string.Empty,
                Tenant = "t",
                Module = "m"
            }.ComputeId());
        }
    }

    public class TestRoleExtensions
    {
        [TestCase("s", "2", "3", ExpectedResult = "s|2|3")]
        [TestCase("s", "2", ExpectedResult = "s|2")]
        [TestCase("s", ExpectedResult = "s")]
        [TestCase(ExpectedResult = null)]
        public string JoinSubModules(params string[] subModules)
        {
            return subModules.JoinSubModules();
        }

        [Test]
        public void JoinSubModules_Err()
        {
            Assert.Catch<ArgumentNullException>(() => new[] {"a", string.Empty}.JoinSubModules());
            Assert.Catch<ArgumentNullException>(() => new[] {"a", string.Empty, "b"}.JoinSubModules());
            Assert.Catch<ArgumentNullException>(() => new[] {string.Empty, "b"}.JoinSubModules());
        }

        [Test]
        public void ConvertToEntity()
        {
            var crm = new CreateRoleModel
            {
                Corp = "c",
                App = "a",
                Env = "e",
                Tenant = "t",
                Module = "m",
                SubModules = new[] {"s", "2"}
            };
            var role = crm.ConvertToEntity();
            Assert.NotNull(role);
            Assert.AreEqual(crm.Corp, role.Corp);
            Assert.AreEqual(crm.App, role.App);
            Assert.AreEqual(crm.Env, role.Env);
            Assert.AreEqual(crm.Tenant, role.Tenant);
            Assert.AreEqual(crm.Module, role.Module);
            Assert.AreEqual("s|2", role.SubModules);
            Assert.AreEqual("c.a.e.t.m.s|2", role.Id);
        }
    }

    public class TestRoleGroup
    {
        [Test]
        public void ToDomainObject()
        {
            var eRg = new RoleGroup
            {
                Name = "rg",
                Corp = "c",
                App = "a",
                Locked = true,
                RoleGroupUsers = Enumerable.Empty<RoleGroupUser>().ToList(),
                RoleRecords = new List<RoleRecord>
                {
                    new RoleRecord
                    {
                        Id = Guid.NewGuid(),
                        RoleId = "c.a.e.t.m",
                        Permission = Permission.Delete,
                        Env = "e",
                        Tenant = "t"
                    }
                }
            };

            var dRg = eRg.ToDomainObject();
            Assert.NotNull(dRg);
            Assert.AreEqual(eRg.Name, dRg.Name);
            Assert.AreEqual(eRg.Corp, dRg.Corp);
            Assert.AreEqual(eRg.App, dRg.App);
            Assert.AreEqual(eRg.Locked, dRg.Locked);
            Assert.AreEqual(eRg.RoleRecords.Count, dRg.Roles.Length);
        }
    }

    public class TestRoleRecordExtensions
    {
        [Test]
        public void ToEntityObject()
        {
            var role = new global::SimpleAuth.Shared.Domains.Role
            {
                RoleId = "c.a.e.t.m",
                Permission = Permission.View
            };

            var rr = role.ToEntityObject();
            Assert.AreEqual(role.RoleId, rr.RoleId);
            Assert.AreEqual(role.Permission, rr.Permission);
        }
    }
}