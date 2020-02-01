using System.Linq;
using NUnit.Framework;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace Test.SimpleAuth.Shared.Test.Models
{
    public class TestRoleGroupResponseModel
    {
        [Test]
        public void Cast()
        {
            var rg = new RoleGroup
            {
                Name = "rg",
                Roles = null
            };

            var res = RoleGroupResponseModel.Cast(rg);
            Assert.NotNull(res);
            Assert.AreEqual(rg.Name, res.Name);
            Assert.IsNotNull(res.Roles);
            Assert.AreEqual(0, res.Roles.Length);

            rg = new RoleGroup
            {
                Roles = new[]
                {
                    new Role
                    {
                        RoleId = "c.a.e.t.m1",
                        Permission = Permission.Add | Permission.Delete
                    },
                    
                    new Role
                    {
                        RoleId = "c.a.e.t.m2",
                        Permission = Permission.View | Permission.Edit
                    }
                }
            };
            
            res = RoleGroupResponseModel.Cast(rg);
            Assert.NotNull(res);
            Assert.IsNotNull(res.Roles);
            Assert.AreEqual(2, res.Roles.Length);
            Assert.AreEqual(rg.Roles.First().RoleId, res.Roles.First().Role);
            Assert.AreEqual(rg.Roles.First().Permission.Serialize(), res.Roles.First().Permission);
            Assert.AreEqual(rg.Roles.Skip(1).First().RoleId, res.Roles.Skip(1).First().Role);
            Assert.AreEqual(rg.Roles.Skip(1).First().Permission.Serialize(), res.Roles.Skip(1).First().Permission);
        }
    }
}