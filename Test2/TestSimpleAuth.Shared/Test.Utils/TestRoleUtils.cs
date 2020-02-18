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

            ClientPermissionModel clientPermissionModel;
            try
            {
                RoleUtils.Parse(roleId, out clientPermissionModel);
                if (expectErr)
                    Assert.Fail("Error expected");
            }
            catch
            {
                if (expectErr)
                    return;
                throw;
            }

            Assert.NotNull(clientPermissionModel.SubModules);

            Assert.AreEqual(splExpected[0], clientPermissionModel.Corp);
            Assert.AreEqual(splExpected[1], clientPermissionModel.App);
            Assert.AreEqual(splExpected[2], clientPermissionModel.Env);
            Assert.AreEqual(splExpected[3], clientPermissionModel.Tenant);
            Assert.AreEqual(splExpected[4], clientPermissionModel.Module);
            Assert.AreEqual(subModulesExpected, clientPermissionModel.SubModules);
        }

        [TestCase("8", Verb.Delete)]
        [TestCase("12", Verb.Delete | Verb.Edit)]
        public void Parse_WithPermission(string inputPermission, Verb expectedVerb)
        {
            RoleUtils.Parse("c.a.e.t.m", inputPermission, out var clientRoleModel);
            Assert.AreEqual(expectedVerb, clientRoleModel.Verb);
        }

        [TestCase("c.a.e.t.m", Verb.Crud, "c.a.e.t.m", Verb.Add, ExpectedResult = true)]
        [TestCase("c.a.e.t.m.s", Verb.Crud, "c.a.e.t.m.s", Verb.Add, ExpectedResult = true)]
        [TestCase("c.a.e.t.m", Verb.Add, "c.a.e.t.m", Verb.Crud, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Verb.Add, "c.a.e.t.m.s", Verb.Crud, ExpectedResult = false)]
        [TestCase("c.a.e.t.m", Verb.Add, "c.a.e.t.m", Verb.Add, ExpectedResult = true)]
        [TestCase("c.a.e.t.m", Verb.Add, "c.a.e.t.m", Verb.Edit, ExpectedResult = false)]
        [TestCase("*.a.e.t.m", Verb.Add, "c.a.e.t.m", Verb.Add, ExpectedResult = true)]
        [TestCase("c.a.e.t.m", Verb.Add, "*.a.e.t.m", Verb.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Verb.Add, "c.a.e.t.m.s|2", Verb.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Verb.Add, "q.a.e.t.m.s", Verb.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Verb.Add, "c.q.e.t.m.s", Verb.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Verb.Add, "c.a.q.t.m.s", Verb.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Verb.Add, "c.a.e.q.m.s", Verb.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Verb.Add, "c.a.e.t.q.s", Verb.Add, ExpectedResult = false)]
        [TestCase("c.a.e.t.m.s", Verb.Add, "c.a.e.t.m.q", Verb.Add, ExpectedResult = false)]
        public bool ContainsOrEquals_All(
            string bigRoleId, Verb bigVerb,
            string smallRoleId, Verb smallVerb)
        {
            RoleUtils.Parse(bigRoleId, bigVerb.Serialize(), out var bigClientRoleModel);
            RoleUtils.Parse(smallRoleId, smallVerb.Serialize(), out var smallClientRoleModel);

            return RoleUtils.ContainsOrEquals(bigClientRoleModel, smallClientRoleModel, RoleUtils.ComparisionFlag.All);
        }

        [Test]
        public void ContainsOrEquals_IgnorePermission()
        {
            var roleId = "c.a.e.t.m";
            RoleUtils.Parse(roleId, Verb.Add.Serialize(), out var big);
            RoleUtils.Parse(roleId, Verb.Edit.Serialize(), out var small);
            
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
            var smallClientRoleModel = new ClientPermissionModel
            {
                Corp = "c",
                App = "a",
                Env = "e",
                Tenant = "t",
                Module = "m",
                Verb = Verb.Add
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
                Crm("c.a.e.t.m", Verb.Edit),
                Crm("c.a.e.t.m", Verb.Add | Verb.Edit),
                Crm("c.a.e.t.m", Verb.Edit),
                Crm("c.a.e.0.m", Verb.Add),
            };

            l = RoleUtils.Distinct(l).ToArray();
            Assert.AreEqual(2, l.Length);

            var e1 = l.First(x => x.Tenant == "t");
            Assert.NotNull(e1);
            Assert.AreEqual(Verb.Add | Verb.Edit, e1.Verb);

            var e2 = l.First(x => x.Tenant == "0");
            Assert.NotNull(e2);
            Assert.AreEqual(Verb.Add, e2.Verb);

            ClientPermissionModel Crm(string roleId, Verb permission)
            {
                RoleUtils.Parse(roleId, out var clientRoleModel);
                clientRoleModel.Verb = permission;
                return clientRoleModel;
            }
        }

        [TestCase("c.a.e.t.m", Verb.Add)]
        [TestCase("c.a.e.t.m", Verb.Crud)]
        [TestCase("c.a.e.t.m", Verb.CurrentMax)]
        [TestCase("c.a.e.t.m", Verb.Full)]
        public void Merge_And_UnMerge(string roleId, Verb verb)
        {
            var merged = RoleUtils.Merge(roleId, verb);
            var unMerged = RoleUtils.UnMerge(merged);
            Assert.AreEqual(roleId, unMerged.Item1);
            Assert.AreEqual(verb, unMerged.Item2);
        }
        
        [Test]
        public void Merge_And_UnMerge_Exception()
        {
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge(null, Verb.Add));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge(string.Empty, Verb.Add));
            Assert.Catch<ArgumentNullException>(() => RoleUtils.Merge("    ", Verb.Add));
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