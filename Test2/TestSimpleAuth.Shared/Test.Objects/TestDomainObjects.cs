using System.Linq;
using NUnit.Framework;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;

namespace Test.SimpleAuth.Shared.Test.Objects
{
    public class TestRole
    {
        [Test]
        public void ToClientRoleModel()
        {
            var role = new Role
            {
                RoleId = "c.a.e.t.m.s",
                Permission = Permission.Add
            };

            var crm = role.ToClientRoleModel();
            Assert.NotNull(crm);
            Assert.AreEqual("c", crm.Corp);
            Assert.AreEqual("a", crm.App);
            Assert.AreEqual("e", crm.Env);
            Assert.AreEqual("t", crm.Tenant);
            Assert.AreEqual("m", crm.Module);
            Assert.AreEqual("s", crm.SubModules.First());
            Assert.AreEqual(role.Permission, crm.Permission);
        }

        [Test]
        public void Cast()
        {
            var role = new Role
            {
                RoleId = "c.a.e.t.m.s",
                Permission = Permission.Add
            };

            var rm = role.Cast();
            Assert.NotNull(rm);
            Assert.AreEqual(role.RoleId, rm.Role);
            Assert.AreEqual("1", rm.Permission);
        }
    }

    public class TestRoleExtensions
    {
        [Test]
        public void DistinctRoles()
        {
            var arr = ArrS("c.a.e.t.m", Permission.Add, Permission.Edit);
            Assert.AreEqual(2, arr.Length);
            
            arr = arr.DistinctRoles().ToArray();
            Assert.AreEqual(1, arr.Length);
            Assert.AreEqual("c.a.e.t.m", arr[0].RoleId);
            Assert.AreEqual(Permission.Add | Permission.Edit, arr[0].Permission);
            
            arr = ArrS("c.a.e.t.m", Permission.Add, Permission.Edit).Concat(ArrS("c.a.*.t.m", Permission.Delete)).ToArray();
            Assert.AreEqual(3, arr.Length);
            
            arr = arr.DistinctRoles().ToArray();
            Assert.AreEqual(2, arr.Length);
            
            arr = ArrS("c.a.*.t.m", Permission.Crud).Concat(ArrS("c.a.e.t.m", Permission.Add, Permission.Edit)).ToArray();
            Assert.AreEqual(3, arr.Length);
            
            arr = arr.DistinctRoles().ToArray();
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual("c.a.*.t.m", arr[0].RoleId);
            Assert.AreEqual(Permission.Crud, arr[0].Permission);
            Assert.AreEqual("c.a.e.t.m", arr[1].RoleId);
            Assert.AreEqual(Permission.Add | Permission.Edit, arr[1].Permission);

            arr = ArrS("c.a.*.t.m", Permission.None);
            Assert.AreEqual(1, arr.Length);
            
            arr = arr.DistinctRoles().ToArray();
            Assert.AreEqual(0, arr.Length);

            Role[] ArrS(string roleId, params Permission[] permissions)
            {
                return permissions.Select(x => new Role
                {
                    RoleId = roleId,
                    Permission = x
                }).ToArray();
            }
        }
    }
}