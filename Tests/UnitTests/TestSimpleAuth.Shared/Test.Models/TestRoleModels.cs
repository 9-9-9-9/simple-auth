using System.Linq;
using NUnit.Framework;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace Test.SimpleAuth.Shared.Test.Models
{
    public class TestRoleModel
    {
        [Test]
        public void Cast()
        {
            var role = new Permission
            {
                RoleId = "c.a.e.t.m",
                Verb = Verb.Delete
            };
            
            var roleModel = PermissionModel.Cast(role);
            Assert.NotNull(roleModel);
            Assert.AreEqual(role.RoleId, roleModel.Role);
            Assert.AreEqual("8", roleModel.Verb);
        }
    }

    public class TestClientRoleModel
    {
        [TestCase("c", "a", "e", "t", "m", Verb.Add, ExpectedResult = "c.a.e.t.m, Permission: Add")]
        [TestCase("c", "a", "e", "t", "m", Verb.Delete, ExpectedResult = "c.a.e.t.m, Permission: Delete")]
        [TestCase("c", "a", "e", "t", "m", Verb.Delete, "s", ExpectedResult = "c.a.e.t.m.s, Permission: Delete")]
        [TestCase("c", "a", "e", "t", "m", Verb.Delete, "s", "2", ExpectedResult = "c.a.e.t.m.s|2, Permission: Delete")]
        public string ToString_Override(string corp, string app, string env, string tenant, string module, Verb verb, params string[] subModules)
        {
            return new ClientPermissionModel
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModules,
                Verb = verb
            }.ToString();
        }

        
        [TestCase("c.a.e.t.m", "c", "a", "e", "t", "m")]
        [TestCase("c.a.e.t.m.s", "c", "a", "e", "t", "m", "s")]
        [TestCase("c.a.e.t.m.s|2", "c", "a", "e", "t", "m", "s", "2")]
        public void ToRole(string expectedRoleId, string corp, string app, string env, string tenant, string module, params string[] subModules)
        {
            var role = new ClientPermissionModel
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModules,
                Verb = Verb.Crud
            }.ToRole();
            
            Assert.NotNull(role);
            Assert.AreEqual(expectedRoleId, role.RoleId);
            Assert.AreEqual(Verb.Crud, role.Verb);
            Assert.IsFalse(role.Locked);
        }

        [Test]
        public void Equals_Override()
        {
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(Crm("c.a.e.t.m", Verb.Add).Equals(Crm("c.a.e.t.m", Verb.Add)));
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(Crm("c.a.e.t.m.s", Verb.Add).Equals(Crm("c.a.e.t.m.s", Verb.Add)));
            
            Assert.IsFalse(Crm("q.a.e.t.m.s", Verb.Add).Equals(Crm("c.a.e.t.m.s", Verb.Add)));
            Assert.IsFalse(Crm("c.q.e.t.m.s", Verb.Add).Equals(Crm("c.a.e.t.m.s", Verb.Add)));
            Assert.IsFalse(Crm("c.a.q.t.m.s", Verb.Add).Equals(Crm("c.a.e.t.m.s", Verb.Add)));
            Assert.IsFalse(Crm("c.a.e.q.m.s", Verb.Add).Equals(Crm("c.a.e.t.m.s", Verb.Add)));
            Assert.IsFalse(Crm("c.a.e.t.q.s", Verb.Add).Equals(Crm("c.a.e.t.m.s", Verb.Add)));
            Assert.IsFalse(Crm("c.a.e.t.m.q", Verb.Add).Equals(Crm("c.a.e.t.m.s", Verb.Add)));
            Assert.IsFalse(Crm("c.a.e.t.m.s", Verb.Edit).Equals(Crm("c.a.e.t.m.s", Verb.Add)));
            Assert.IsFalse(Crm("c.a.e.t.q.s|3", Verb.Add).Equals(Crm("c.a.e.t.m.s|2", Verb.Add)));

            ClientPermissionModel Crm(string roleId, Verb permission)
            {
                return ClientPermissionModel.From(roleId, permission);
            }
        }

        [TestCase("c", "a", "e", "t", "m", ExpectedResult = "c.a.e.t.m")]
        [TestCase("c", "a", "e", "t", "m", "s", ExpectedResult = "c.a.e.t.m.s")]
        [TestCase("c", "a", "e", "t", "m", "s", "2", ExpectedResult = "c.a.e.t.m.s|2")]
        public string ComputeId(string corp, string app, string env, string tenant, string module, params string[] subModules)
        {
            return new ClientPermissionModel
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModules,   
            }.ComputeId();
        }

        [Test]
        public void DistinctRoles()
        {
            var distinct = new[]
            {
                ClientPermissionModel.From("c.a.e.t.m", Verb.Add),
                ClientPermissionModel.From("c.a.e.t.m", Verb.Edit)
            }.DistinctPermissions().ToArray();
            
            Assert.AreEqual(2, distinct.Length);
            
            distinct = new[]
            {
                ClientPermissionModel.From("c.a.e.t.m", Verb.Add),
                ClientPermissionModel.From("c.a.e.t.m", Verb.Add)
            }.DistinctPermissions().ToArray();
            
            Assert.AreEqual(1, distinct.Length);
            
            Assert.AreEqual("c", distinct.First().Corp);
            Assert.AreEqual("a", distinct.First().App);
            Assert.AreEqual("e", distinct.First().Env);
            Assert.AreEqual("t", distinct.First().Tenant);
            Assert.AreEqual("m", distinct.First().Module);
            Assert.AreEqual(Verb.Add, distinct.First().Verb);
            
            distinct = new[]
            {
                ClientPermissionModel.From("c.a.e.t.m", Verb.Add),
                ClientPermissionModel.From("c.a.e.t.m", Verb.Crud)
            }.DistinctPermissions().ToArray();
            
            Assert.AreEqual(1, distinct.Length);
            
            Assert.AreEqual("c", distinct.First().Corp);
            Assert.AreEqual("a", distinct.First().App);
            Assert.AreEqual("e", distinct.First().Env);
            Assert.AreEqual("t", distinct.First().Tenant);
            Assert.AreEqual("m", distinct.First().Module);
            Assert.AreEqual(Verb.Crud, distinct.First().Verb);
            
            distinct = new[]
            {
                ClientPermissionModel.From("c.a.e.t.m", "7"),
                ClientPermissionModel.From("c.a.e.*.m", Verb.Crud)
            }.DistinctPermissions().ToArray();
            
            Assert.AreEqual(1, distinct.Length);
            
            Assert.AreEqual("c", distinct.First().Corp);
            Assert.AreEqual("a", distinct.First().App);
            Assert.AreEqual("e", distinct.First().Env);
            Assert.AreEqual("*", distinct.First().Tenant);
            Assert.AreEqual("m", distinct.First().Module);
            Assert.AreEqual(Verb.Crud, distinct.First().Verb);
            
            distinct = new[]
            {
                ClientPermissionModel.From("c.a.e.t.m", Verb.Add),
                ClientPermissionModel.From("c.a.e.*.m", Verb.Edit)
            }.DistinctPermissions().ToArray();
            
            Assert.AreEqual(2, distinct.Length);
        }
    }
}