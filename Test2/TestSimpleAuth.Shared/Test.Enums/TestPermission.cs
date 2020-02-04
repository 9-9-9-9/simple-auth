using System;
using NUnit.Framework;
using SimpleAuth.Shared.Enums;

namespace Test.SimpleAuth.Shared.Test.Enums
{
    public class TestPermission
    {
        [TestCase(Permission.None, ExpectedResult = "0")]
        [TestCase(Permission.Add, ExpectedResult = "1")]
        [TestCase(Permission.View, ExpectedResult = "2")]
        [TestCase(Permission.Edit, ExpectedResult = "4")]
        [TestCase(Permission.Delete, ExpectedResult = "8")]
        [TestCase(Permission.Add | Permission.View, ExpectedResult = "3")]
        [TestCase(Permission.Crud, ExpectedResult = "15")]
        [TestCase(Permission.CurrentMax, ExpectedResult = "15")]
        [TestCase(Permission.Full, ExpectedResult = "*")]
        public string Serialize(Permission p)
        {
            return p.Serialize();
        }

        [TestCase("0", ExpectedResult = Permission.None)]
        [TestCase("", ExpectedResult = Permission.None)]
        [TestCase("0 ", ExpectedResult = Permission.None)]
        [TestCase("* ", ExpectedResult = Permission.None)]
        [TestCase("1", ExpectedResult = Permission.Add)]
        [TestCase("2", ExpectedResult = Permission.View)]
        [TestCase("4", ExpectedResult = Permission.Edit)]
        [TestCase("8", ExpectedResult = Permission.Delete)]
        [TestCase("3", ExpectedResult = Permission.Add | Permission.View)]
        [TestCase("15", ExpectedResult = Permission.Crud)]
        [TestCase("15", ExpectedResult = Permission.CurrentMax)]
        [TestCase("*", ExpectedResult = Permission.Full)]
        public Permission Deserialize(string serialized)
        {
            try
            {
                return serialized.Deserialize();
            }
            catch (FormatException)
            {
                return Permission.None;
            }
        }

        [TestCase(Permission.Add, Permission.Add, ExpectedResult = Permission.Add)]
        [TestCase(Permission.Add, Permission.Edit, ExpectedResult = Permission.Add | Permission.Edit)]
        [TestCase(Permission.Add, Permission.Edit, Permission.Delete, ExpectedResult =
            Permission.Add | Permission.Edit | Permission.Delete)]
        [TestCase(Permission.Add, Permission.Edit, Permission.Add, ExpectedResult = Permission.Add | Permission.Edit)]
        [TestCase(Permission.Crud, Permission.Add, ExpectedResult = Permission.Crud)]
        [TestCase(Permission.CurrentMax, Permission.Add, ExpectedResult = Permission.CurrentMax)]
        [TestCase(Permission.Add, Permission.Crud, ExpectedResult = Permission.Crud)]
        [TestCase(Permission.Add, Permission.CurrentMax, ExpectedResult = Permission.CurrentMax)]
        public Permission Grant(Permission src, params Permission[] subPermissions)
        {
            return src.Grant(subPermissions);
        }

        [TestCase(Permission.Full, Permission.Add, ExpectedResult =
            Permission.View | Permission.Edit | Permission.Delete)]
        [TestCase(Permission.CurrentMax, Permission.Add, ExpectedResult =
            Permission.View | Permission.Edit | Permission.Delete)]
        [TestCase(Permission.Crud, Permission.Add, ExpectedResult =
            Permission.View | Permission.Edit | Permission.Delete)]
        [TestCase(Permission.Add | Permission.Edit, Permission.Edit, ExpectedResult = Permission.Add)]
        [TestCase(Permission.Add | Permission.Edit, Permission.Delete, ExpectedResult =
            Permission.Add | Permission.Edit)]
        public Permission Revoke(Permission src, params Permission[] subPermissions)
        {
            return src.Revoke(subPermissions);
        }

        [Test]
        public void Revoke_2()
        {
            for (var i = 0; i <= 255; i++)
            {
                var p = i.ToString().Deserialize();
                Assert.AreEqual(p, Permission.None.Grant(p));
                Assert.AreEqual(p, p.Grant(Permission.None));
                Assert.AreEqual(Permission.None, Permission.None.Revoke(p));
                if (i > 0)
                    Assert.AreNotEqual(Permission.None, p.Revoke(Permission.None));
            }
        }
    }
}