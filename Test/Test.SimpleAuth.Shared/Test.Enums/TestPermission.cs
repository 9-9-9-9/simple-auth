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
    }
}