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
            var role = new Permission
            {
                RoleId = "c.a.e.t.m.s",
                Verb = Verb.Add
            };

            var crm = role.ToClientPermissionModel();
            Assert.NotNull(crm);
            Assert.AreEqual("c", crm.Corp);
            Assert.AreEqual("a", crm.App);
            Assert.AreEqual("e", crm.Env);
            Assert.AreEqual("t", crm.Tenant);
            Assert.AreEqual("m", crm.Module);
            Assert.AreEqual("s", crm.SubModules.First());
            Assert.AreEqual(role.Verb, crm.Verb);
        }

        [Test]
        public void Cast()
        {
            var role = new Permission
            {
                RoleId = "c.a.e.t.m.s",
                Verb = Verb.Add
            };

            var rm = role.Cast();
            Assert.NotNull(rm);
            Assert.AreEqual(role.RoleId, rm.Role);
            Assert.AreEqual("1", rm.Verb);
        }
    }

    public class TestRoleExtensions
    {
        [Test]
        public void DistinctRoles()
        {
            var arr = ArrS("c.a.e.t.m", Verb.Add, Verb.Edit);
            Assert.AreEqual(2, arr.Length);
            
            arr = arr.DistinctRoles().ToArray();
            Assert.AreEqual(1, arr.Length);
            Assert.AreEqual("c.a.e.t.m", arr[0].RoleId);
            Assert.AreEqual(Verb.Add | Verb.Edit, arr[0].Verb);
            
            arr = ArrS("c.a.e.t.m", Verb.Add, Verb.Edit).Concat(ArrS("c.a.*.t.m", Verb.Delete)).ToArray();
            Assert.AreEqual(3, arr.Length);
            
            arr = arr.DistinctRoles().ToArray();
            Assert.AreEqual(2, arr.Length);
            
            arr = ArrS("c.a.*.t.m", Verb.Crud).Concat(ArrS("c.a.e.t.m", Verb.Add, Verb.Edit)).ToArray();
            Assert.AreEqual(3, arr.Length);
            
            arr = arr.DistinctRoles().ToArray();
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual("c.a.*.t.m", arr[0].RoleId);
            Assert.AreEqual(Verb.Crud, arr[0].Verb);
            Assert.AreEqual("c.a.e.t.m", arr[1].RoleId);
            Assert.AreEqual(Verb.Add | Verb.Edit, arr[1].Verb);

            arr = ArrS("c.a.*.t.m", Verb.None);
            Assert.AreEqual(1, arr.Length);
            
            arr = arr.DistinctRoles().ToArray();
            Assert.AreEqual(0, arr.Length);

            Permission[] ArrS(string roleId, params Verb[] permissions)
            {
                return permissions.Select(x => new Permission
                {
                    RoleId = roleId,
                    Verb = x
                }).ToArray();
            }
        }
    }
}