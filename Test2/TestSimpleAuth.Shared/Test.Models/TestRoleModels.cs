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
            var role = new Role
            {
                RoleId = "c.a.e.t.m",
                Permission = Permission.Delete
            };
            
            var roleModel = RoleModel.Cast(role);
            Assert.NotNull(roleModel);
            Assert.AreEqual(role.RoleId, roleModel.Role);
            Assert.AreEqual("8", roleModel.Permission);
        }
    }

    public class TestClientRoleModel
    {
        [TestCase("c", "a", "e", "t", "m", Permission.Add, ExpectedResult = "c.a.e.t.m, Permission: Add")]
        [TestCase("c", "a", "e", "t", "m", Permission.Delete, ExpectedResult = "c.a.e.t.m, Permission: Delete")]
        [TestCase("c", "a", "e", "t", "m", Permission.Delete, "s", ExpectedResult = "c.a.e.t.m.s, Permission: Delete")]
        [TestCase("c", "a", "e", "t", "m", Permission.Delete, "s", "2", ExpectedResult = "c.a.e.t.m.s|2, Permission: Delete")]
        public string ToString_Override(string corp, string app, string env, string tenant, string module, Permission permission, params string[] subModules)
        {
            return new ClientRoleModel
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModules,
                Permission = permission
            }.ToString();
        }

        
        [TestCase("c.a.e.t.m", "c", "a", "e", "t", "m")]
        [TestCase("c.a.e.t.m.s", "c", "a", "e", "t", "m", "s")]
        [TestCase("c.a.e.t.m.s|2", "c", "a", "e", "t", "m", "s", "2")]
        public void ToRole(string expectedRoleId, string corp, string app, string env, string tenant, string module, params string[] subModules)
        {
            var role = new ClientRoleModel
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModules,
                Permission = Permission.Crud
            }.ToRole();
            
            Assert.NotNull(role);
            Assert.AreEqual(expectedRoleId, role.RoleId);
            Assert.AreEqual(Permission.Crud, role.Permission);
            Assert.IsFalse(role.Locked);
        }

        [Test]
        public void Equals_Override()
        {
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(Crm("c.a.e.t.m", Permission.Add).Equals(Crm("c.a.e.t.m", Permission.Add)));
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(Crm("c.a.e.t.m.s", Permission.Add).Equals(Crm("c.a.e.t.m.s", Permission.Add)));
            
            Assert.IsFalse(Crm("q.a.e.t.m.s", Permission.Add).Equals(Crm("c.a.e.t.m.s", Permission.Add)));
            Assert.IsFalse(Crm("c.q.e.t.m.s", Permission.Add).Equals(Crm("c.a.e.t.m.s", Permission.Add)));
            Assert.IsFalse(Crm("c.a.q.t.m.s", Permission.Add).Equals(Crm("c.a.e.t.m.s", Permission.Add)));
            Assert.IsFalse(Crm("c.a.e.q.m.s", Permission.Add).Equals(Crm("c.a.e.t.m.s", Permission.Add)));
            Assert.IsFalse(Crm("c.a.e.t.q.s", Permission.Add).Equals(Crm("c.a.e.t.m.s", Permission.Add)));
            Assert.IsFalse(Crm("c.a.e.t.m.q", Permission.Add).Equals(Crm("c.a.e.t.m.s", Permission.Add)));
            Assert.IsFalse(Crm("c.a.e.t.m.s", Permission.Edit).Equals(Crm("c.a.e.t.m.s", Permission.Add)));
            Assert.IsFalse(Crm("c.a.e.t.q.s|3", Permission.Add).Equals(Crm("c.a.e.t.m.s|2", Permission.Add)));

            ClientRoleModel Crm(string roleId, Permission permission)
            {
                return ClientRoleModel.From(roleId, permission);
            }
        }

        [TestCase("c", "a", "e", "t", "m", ExpectedResult = "c.a.e.t.m")]
        [TestCase("c", "a", "e", "t", "m", "s", ExpectedResult = "c.a.e.t.m.s")]
        [TestCase("c", "a", "e", "t", "m", "s", "2", ExpectedResult = "c.a.e.t.m.s|2")]
        public string ComputeId(string corp, string app, string env, string tenant, string module, params string[] subModules)
        {
            return new ClientRoleModel
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
                ClientRoleModel.From("c.a.e.t.m", Permission.Add),
                ClientRoleModel.From("c.a.e.t.m", Permission.Edit)
            }.DistinctRoles().ToArray();
            
            Assert.AreEqual(2, distinct.Length);
            
            distinct = new[]
            {
                ClientRoleModel.From("c.a.e.t.m", Permission.Add),
                ClientRoleModel.From("c.a.e.t.m", Permission.Add)
            }.DistinctRoles().ToArray();
            
            Assert.AreEqual(1, distinct.Length);
            
            Assert.AreEqual("c", distinct.First().Corp);
            Assert.AreEqual("a", distinct.First().App);
            Assert.AreEqual("e", distinct.First().Env);
            Assert.AreEqual("t", distinct.First().Tenant);
            Assert.AreEqual("m", distinct.First().Module);
            Assert.AreEqual(Permission.Add, distinct.First().Permission);
            
            distinct = new[]
            {
                ClientRoleModel.From("c.a.e.t.m", Permission.Add),
                ClientRoleModel.From("c.a.e.t.m", Permission.Crud)
            }.DistinctRoles().ToArray();
            
            Assert.AreEqual(1, distinct.Length);
            
            Assert.AreEqual("c", distinct.First().Corp);
            Assert.AreEqual("a", distinct.First().App);
            Assert.AreEqual("e", distinct.First().Env);
            Assert.AreEqual("t", distinct.First().Tenant);
            Assert.AreEqual("m", distinct.First().Module);
            Assert.AreEqual(Permission.Crud, distinct.First().Permission);
            
            distinct = new[]
            {
                ClientRoleModel.From("c.a.e.t.m", "7"),
                ClientRoleModel.From("c.a.e.*.m", Permission.Crud)
            }.DistinctRoles().ToArray();
            
            Assert.AreEqual(1, distinct.Length);
            
            Assert.AreEqual("c", distinct.First().Corp);
            Assert.AreEqual("a", distinct.First().App);
            Assert.AreEqual("e", distinct.First().Env);
            Assert.AreEqual("*", distinct.First().Tenant);
            Assert.AreEqual("m", distinct.First().Module);
            Assert.AreEqual(Permission.Crud, distinct.First().Permission);
            
            distinct = new[]
            {
                ClientRoleModel.From("c.a.e.t.m", Permission.Add),
                ClientRoleModel.From("c.a.e.*.m", Permission.Edit)
            }.DistinctRoles().ToArray();
            
            Assert.AreEqual(2, distinct.Length);
        }
    }
}