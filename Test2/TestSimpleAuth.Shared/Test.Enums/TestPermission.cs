using System;
using NUnit.Framework;
using SimpleAuth.Shared.Enums;

namespace Test.SimpleAuth.Shared.Test.Enums
{
    public class TestPermission
    {
        [TestCase(Verb.None, ExpectedResult = "0")]
        [TestCase(Verb.Add, ExpectedResult = "1")]
        [TestCase(Verb.View, ExpectedResult = "2")]
        [TestCase(Verb.Edit, ExpectedResult = "4")]
        [TestCase(Verb.Delete, ExpectedResult = "8")]
        [TestCase(Verb.Add | Verb.View, ExpectedResult = "3")]
        [TestCase(Verb.Crud, ExpectedResult = "15")]
        [TestCase(Verb.CurrentMax, ExpectedResult = "15")]
        [TestCase(Verb.Full, ExpectedResult = "*")]
        public string Serialize(Verb p)
        {
            return p.Serialize();
        }

        [TestCase("0", ExpectedResult = Verb.None)]
        [TestCase("", ExpectedResult = Verb.None)]
        [TestCase("0 ", ExpectedResult = Verb.None)]
        [TestCase("* ", ExpectedResult = Verb.None)]
        [TestCase("1", ExpectedResult = Verb.Add)]
        [TestCase("2", ExpectedResult = Verb.View)]
        [TestCase("4", ExpectedResult = Verb.Edit)]
        [TestCase("8", ExpectedResult = Verb.Delete)]
        [TestCase("3", ExpectedResult = Verb.Add | Verb.View)]
        [TestCase("15", ExpectedResult = Verb.Crud)]
        [TestCase("15", ExpectedResult = Verb.CurrentMax)]
        [TestCase("*", ExpectedResult = Verb.Full)]
        public Verb Deserialize(string serialized)
        {
            try
            {
                return serialized.Deserialize();
            }
            catch (FormatException)
            {
                return Verb.None;
            }
        }

        [TestCase(Verb.Add, Verb.Add, ExpectedResult = Verb.Add)]
        [TestCase(Verb.Add, Verb.Edit, ExpectedResult = Verb.Add | Verb.Edit)]
        [TestCase(Verb.Add, Verb.Edit, Verb.Delete, ExpectedResult =
            Verb.Add | Verb.Edit | Verb.Delete)]
        [TestCase(Verb.Add, Verb.Edit, Verb.Add, ExpectedResult = Verb.Add | Verb.Edit)]
        [TestCase(Verb.Crud, Verb.Add, ExpectedResult = Verb.Crud)]
        [TestCase(Verb.CurrentMax, Verb.Add, ExpectedResult = Verb.CurrentMax)]
        [TestCase(Verb.Add, Verb.Crud, ExpectedResult = Verb.Crud)]
        [TestCase(Verb.Add, Verb.CurrentMax, ExpectedResult = Verb.CurrentMax)]
        public Verb Grant(Verb src, params Verb[] subPermissions)
        {
            return src.Grant(subPermissions);
        }

        [TestCase(Verb.Full, Verb.Add, ExpectedResult =
            Verb.View | Verb.Edit | Verb.Delete)]
        [TestCase(Verb.CurrentMax, Verb.Add, ExpectedResult =
            Verb.View | Verb.Edit | Verb.Delete)]
        [TestCase(Verb.Crud, Verb.Add, ExpectedResult =
            Verb.View | Verb.Edit | Verb.Delete)]
        [TestCase(Verb.Add | Verb.Edit, Verb.Edit, ExpectedResult = Verb.Add)]
        [TestCase(Verb.Add | Verb.Edit, Verb.Delete, ExpectedResult =
            Verb.Add | Verb.Edit)]
        public Verb Revoke(Verb src, params Verb[] subPermissions)
        {
            return src.Revoke(subPermissions);
        }

        [Test]
        public void Revoke_2()
        {
            for (var i = 0; i <= 255; i++)
            {
                var p = i.ToString().Deserialize();
                Assert.AreEqual(p, Verb.None.Grant(p));
                Assert.AreEqual(p, p.Grant(Verb.None));
                Assert.AreEqual(Verb.None, Verb.None.Revoke(p));
                if (i > 0)
                    Assert.AreNotEqual(Verb.None, p.Revoke(Verb.None));
            }
        }
    }
}