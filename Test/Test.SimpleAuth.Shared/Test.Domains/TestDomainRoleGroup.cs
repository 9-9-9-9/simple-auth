using System.Linq;
using NUnit.Framework;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;

namespace Test.SimpleAuth.Shared.Domains
{
    public class TestDomainRoleGroupExtensions
    {
        [Test]
        public void DistinctRoles()
        {
            Assert.IsEmpty(new Role[0].DistinctRoles());
            Assert.IsEmpty(((Role[]) null).DistinctRoles());
            Assert.IsEmpty(new[]
            {
                new Role
                {
                    RoleId = "a",
                    Permission = Permission.None
                },
                new Role
                {
                    RoleId = "b",
                    Permission = Permission.None
                }
            }.DistinctRoles());
            Assert.AreEqual(1, new[]
            {
                new Role
                {
                    RoleId = "a",
                    Permission = Permission.None
                },
                new Role
                {
                    RoleId = "b",
                    Permission = Permission.Add
                }
            }.DistinctRoles().Count());

            var res = new[]
            {
                new Role
                {
                    RoleId = "b",
                    Permission = Permission.Add
                },
                new Role
                {
                    RoleId = "b",
                    Permission = Permission.Edit
                },
                new Role
                {
                    RoleId = "b",
                    Permission = Permission.Add
                }
            }.DistinctRoles().ToArray();
            Assert.AreEqual(1, res.Length);
            Assert.AreEqual("b", res.First().RoleId);
            Assert.AreEqual(Permission.Add.Grant(Permission.Edit), res.First().Permission);
        }
    }
}