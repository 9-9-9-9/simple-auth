using System;
using System.Linq;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace Test.SimpleAuth.Shared.Test.Utils
{
    public class TestRoleUtils
    {
        private const string Err = "#Error$";

        [TestCase("c", "a", "e", "t", "m", "s", ExpectedResult = "c.a.e.t.m.s")]
        [TestCase("c", "a", "e", "t", "m", "", ExpectedResult = "c.a.e.t.m")]
        [TestCase("c", "a", "e", "t", "m", null, ExpectedResult = "c.a.e.t.m")]
        [TestCase("C", "a", "E", "t", "M", "s", ExpectedResult = "c.a.e.t.m.s")]
        [TestCase("c", "a", "e", "t", null, "s", ExpectedResult = Err)]
        [TestCase("c", "a", "e", "", "m", "s", ExpectedResult = Err)]
        [TestCase("c", "a", "    ", "t", "m", "s", ExpectedResult = Err)]
        [TestCase("c", "*", "e", "t", "m", "s", ExpectedResult = "c.*.e.t.m.s")]
        [TestCase("*", "a", "e", "t", "m", "s", ExpectedResult = "*.a.e.t.m.s")]
        public string ComputeRoleId(string corp, string app, string env, string tenant, string module,
            string subModule)
        {
            try
            {
                return RoleUtils.ComputeRoleId(corp, app, env, tenant, module, joinedSubModules: subModule);
            }
            catch (ArgumentNullException)
            {
                return Err;
            }
        }

        [TestCase("c", "a", "e", "t", "m", "s", "2", ExpectedResult = "c.a.e.t.m.s|2")]
        [TestCase("c", "a", "e", "t", "m", "s", "2", "3", ExpectedResult = "c.a.e.t.m.s|2|3")]
        [TestCase("c", "a", "e", "t", "m", null, "2", "3", ExpectedResult = Err)]
        [TestCase("c", "a", "e", "t", "m", "s", "", "3", ExpectedResult = Err)]
        public string ComputeRoleId(string corp, string app, string env, string tenant, string module,
            params string[] subModules)
        {
            try
            {
                return RoleUtils.ComputeRoleId(corp, app, env, tenant, module, subModules);
            }
            catch (ArgumentNullException)
            {
                return Err;
            }
        }

        [TestCase("a", ExpectedResult = "a")]
        [TestCase("a", "b", ExpectedResult = "a|b")]
        [TestCase("c", "b", "c", ExpectedResult = "c|b|c")]
        [TestCase(null, ExpectedResult = null)]
        [TestCase("", ExpectedResult = null)]
        [TestCase("", "b", "c", ExpectedResult = Err)]
        [TestCase("a", "    ", "c", ExpectedResult = Err)]
        public string JoinSubModules(params string[] subModules)
        {
            try
            {
                return RoleUtils.JoinSubModules(subModules);
            }
            catch (ArgumentNullException)
            {
                return Err;
            }
        }

        [TestCase("c", "a", "e", "t", " ")]
        [TestCase("c", "a", "e", " ", "m")]
        [TestCase("c", "a", " ", "t", "m")]
        [TestCase("c", " ", "e", "t", "m")]
        [TestCase(" ", "a", "e", "t", "m")]
        public void ThrowIfBlank(string corp, string app, string env, string tenant, string module)
        {
            try
            {
                RoleUtils.ComputeRoleId(corp, app, env, tenant, module, "s");
                Assert.Fail($"Expect {nameof(ArgumentNullException)}");
            }
            catch (ArgumentNullException)
            {
                // OK
            }
        }

        [TestCase("c#a#e#t#m", "c.a.e.t.m")]
        [TestCase("c#a#e#t#m#s", "c.a.e.t.m.s")]
        [TestCase("c#a#e#t#m#s#2", "c.a.e.t.m.s|2")]
        [TestCase("c#a#e#t#m#s#2#3", "c.a.e.t.m.s|2|3")]
        [TestCase("", "c.a.e.t", true)]
        [TestCase("", "c.a.e.t.m.s.2", true)]
        [TestCase("", "c.a.e.t.m.", true)]
        public void Parse(string expectedValue, string roleId, bool expectErr = false)
        {
            var splExpected = expectedValue.Split("#", 6);
            var subModulesExpected = (splExpected.Skip(5).FirstOrDefault()?.Split("#")).OrEmpty().ToArray();

            ClientRoleModel clientRoleModel;
            try
            {
                RoleUtils.Parse(roleId, out clientRoleModel);
                if (expectErr)
                    Assert.Fail("Error expected");
            }
            catch
            {
                if (expectErr)
                    return;
                throw;
            }

            Assert.NotNull(clientRoleModel.SubModules);

            Assert.AreEqual(splExpected[0], clientRoleModel.Corp);
            Assert.AreEqual(splExpected[1], clientRoleModel.App);
            Assert.AreEqual(splExpected[2], clientRoleModel.Env);
            Assert.AreEqual(splExpected[3], clientRoleModel.Tenant);
            Assert.AreEqual(splExpected[4], clientRoleModel.Module);
            Assert.AreEqual(subModulesExpected, clientRoleModel.SubModules);
        }

        [TestCase("8", Permission.Delete)]
        [TestCase("12", Permission.Delete | Permission.Edit)]
        public void Parse_WithPermission(string inputPermission, Permission expectedPermission)
        {
            RoleUtils.Parse("c.a.e.t.m", inputPermission, out var clientRoleModel);
            Assert.AreEqual(expectedPermission, clientRoleModel.Permission);
        }

        [TestCase("c.a.e.t.m", Permission.Crud, "c.a.e.t.m", Permission.Add, ExpectedResult = true)]
        [TestCase("c.a.e.t.m.s", Permission.Crud, "c.a.e.t.m.s", Permission.Add, ExpectedResult = true)]
        [TestCase("c.a.e.t.m", Permission.Add, "c.a.e.t.m", Permission.Crud, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Permission.Add, "c.a.e.t.m.s", Permission.Crud, ExpectedResult = false)]
        [TestCase("c.a.e.t.m", Permission.Add, "c.a.e.t.m", Permission.Add, ExpectedResult = true)]
        [TestCase("c.a.e.t.m", Permission.Add, "c.a.e.t.m", Permission.Edit, ExpectedResult = false)]
        [TestCase("*.a.e.t.m", Permission.Add, "c.a.e.t.m", Permission.Add, ExpectedResult = true)]
        [TestCase("c.a.e.t.m", Permission.Add, "*.a.e.t.m", Permission.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Permission.Add, "c.a.e.t.m.s|2", Permission.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Permission.Add, "q.a.e.t.m.s", Permission.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Permission.Add, "c.q.e.t.m.s", Permission.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Permission.Add, "c.a.q.t.m.s", Permission.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Permission.Add, "c.a.e.q.m.s", Permission.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Permission.Add, "c.a.e.t.q.s", Permission.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Permission.Add, "c.a.e.t.m.q", Permission.Add, ExpectedResult = false)]
        public bool ContainsOrEquals_All(
            string bigRoleId, Permission bigPermission,
            string smallRoleId, Permission smallPermission)
        {
            RoleUtils.Parse(bigRoleId, bigPermission.Serialize(), out var bigClientRoleModel);
            RoleUtils.Parse(smallRoleId, smallPermission.Serialize(), out var smallClientRoleModel);

            return RoleUtils.ContainsOrEquals(bigClientRoleModel, smallClientRoleModel, RoleUtils.ComparisionFlag.All);
        }

        [Test]
        public void ContainsOrEquals_IgnorePermission()
        {
            var roleId = "c.a.e.t.m";
            RoleUtils.Parse(roleId, Permission.Add.Serialize(), out var big);
            RoleUtils.Parse(roleId, Permission.Edit.Serialize(), out var small);
            
            Assert.IsFalse(RoleUtils.ContainsOrEquals(big, small, RoleUtils.ComparisionFlag.All));
            Assert.IsTrue(RoleUtils.ContainsOrEquals(big, small, RoleUtils.ComparisionFlag.Corp | RoleUtils.ComparisionFlag.App | RoleUtils.ComparisionFlag.Env | RoleUtils.ComparisionFlag.Tenant | RoleUtils.ComparisionFlag.Module | RoleUtils.ComparisionFlag.SubModule));
        }

        [TestCase(false, "c.a.e.t.m")]
        [TestCase(true, "c.a.e.t.")]
        [TestCase(true, "c.a.e..m")]
        [TestCase(true, "c.a..t.m")]
        [TestCase(true, "c..e.t.m")]
        [TestCase(true, ".a.e.t.m")]
        public void ContainsOrEquals_ArgumentNullException(bool expectedError, string bigRoleId)
        {
            RoleUtils.Parse(bigRoleId, "1", out var bigClientRoleModel);
            var smallClientRoleModel = new ClientRoleModel
            {
                Corp = "c",
                App = "a",
                Env = "e",
                Tenant = "t",
                Module = "m",
                Permission = Permission.Add
            };
            var err = false;
            try
            {
                RoleUtils.ContainsOrEquals(bigClientRoleModel, smallClientRoleModel, RoleUtils.ComparisionFlag.All);
            }
            catch (ArgumentNullException)
            {
                err = true;
            }
            try
            {
                RoleUtils.ContainsOrEquals(smallClientRoleModel, bigClientRoleModel, RoleUtils.ComparisionFlag.All);
            }
            catch (ArgumentNullException)
            {
                err = true;
            }
            Assert.AreEqual(expectedError, err);
        }

        [Test]
        public void Distinct()
        {
            var l = new[]
            {
                Crm("c.a.e.t.m", Permission.Edit),
                Crm("c.a.e.t.m", Permission.Add | Permission.Edit),
                Crm("c.a.e.t.m", Permission.Edit),
                Crm("c.a.e.0.m", Permission.Add),
            };

            l = RoleUtils.Distinct(l).ToArray();
            Assert.AreEqual(2, l.Length);

            var e1 = l.First(x => x.Tenant == "t");
            Assert.NotNull(e1);
            Assert.AreEqual(Permission.Add | Permission.Edit, e1.Permission);

            var e2 = l.First(x => x.Tenant == "0");
            Assert.NotNull(e2);
            Assert.AreEqual(Permission.Add, e2.Permission);

            ClientRoleModel Crm(string roleId, Permission permission)
            {
                RoleUtils.Parse(roleId, out var clientRoleModel);
                clientRoleModel.Permission = permission;
                return clientRoleModel;
            }
        }

        [TestCase("c.a.e.t.m", Permission.Add)]
        [TestCase("c.a.e.t.m", Permission.Crud)]
        [TestCase("c.a.e.t.m", Permission.CurrentMax)]
        [TestCase("c.a.e.t.m", Permission.Full)]
        public void Merge_And_UnMerge(string roleId, Permission permission)
        {
            var merged = RoleUtils.Merge(roleId, permission);
            var unMerged = RoleUtils.UnMerge(merged);
            Assert.AreEqual(roleId, unMerged.Item1);
            Assert.AreEqual(permission, unMerged.Item2);
        }
        
        [Test]
        public void Merge_And_UnMerge_Exception()
        {
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge(null, Permission.Add));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge(string.Empty, Permission.Add));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge("    ", Permission.Add));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge("c.a.e.t.m", null));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge("c.a.e.t.m", string.Empty));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge("c.a.e.t.m", "    "));
            Assert.Catch<ArgumentException>(() => RoleUtils.Merge("c.a.e.t.m", "0"));

            Assert.Catch<ArgumentNullException>(() => RoleUtils.UnMerge(null));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.UnMerge(string.Empty));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.UnMerge("    "));
            Assert.Catch<ArgumentException>(() => RoleUtils.UnMerge("#"));
            Assert.Catch<ArgumentException>(() => RoleUtils.UnMerge("a#"));
            Assert.Catch<ArgumentException>(() => RoleUtils.UnMerge("##"));
        }
    }
}