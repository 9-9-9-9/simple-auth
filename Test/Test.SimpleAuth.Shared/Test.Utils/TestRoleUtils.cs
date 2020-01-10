using System;
using System.Linq;
using NUnit.Framework;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Utils;

namespace Test.SimpleAuth.Shared.Test.Utils
{
    public class TestRoleUtils
    {
        private const string Delimiter = "`";

        [TestCase(2, "0`1", "*.*.*.*.*`View", "*.*.*.*.*`Edit")]
        [TestCase(1, "0", "*.*.*.*.*`Full", "*.*.*.*.b`View")]
        [TestCase(1, "1", "*.*.*.*.b`View", "*.*.*.*.*`Full")]
        [TestCase(1, "1", "*.*.*.*.b`View", "*.*.*.*.*`Full", "*.*.*.*.a`View", "*.*.*.*.b`Edit")]
        public void TestDistinct(int expectedNumberOfRolesRemaining, string expectRemainingIndexes,
            params string[] roles)
        {
            var clientRoleModels = roles
                .Select(x => x.Split("`"))
                .Select(x =>
                {
                    RoleUtils.Parse(x[0], out var clientRoleModel);
                    clientRoleModel.Permission = (Permission) Enum.Parse(typeof(Permission), x[1]);
                    return clientRoleModel;
                }).ToArray();

            clientRoleModels = RoleUtils.Distinct(clientRoleModels).ToArray();

            Assert.AreEqual(expectedNumberOfRolesRemaining, clientRoleModels.Length);

            var remaining = clientRoleModels.Select(x => $"{x.ComputeId()}{Delimiter}{x.Permission}").ToList();
            foreach (var idx in expectRemainingIndexes.Split(Delimiter).Select(int.Parse))
                Assert.IsTrue(remaining.Contains(roles[idx]));
        }

        [TestCase("c.a.e.t.m", "c", "a", "e", "t", "m", "")]
        [TestCase("c.a.e.t.m", "c", "a", "e", "t", "m", null)]
        [TestCase("c.a.e.t.m.s", "c", "a", "e", "t", "m", "s")]
        [TestCase("c.a.e.t.m.s|s2", "c", "a", "e", "t", "m", "s", "s2")]
        public void TestComputeRoleId(string roleId, string corp, string app, string env, string tenant, string module,
            params string[] subModules)
        {
            Assert.AreEqual(roleId, RoleUtils.ComputeRoleId(corp, app, env, tenant, module, subModules));
        }

        [TestCase("c.a.e.t.m", "c", "a", "e", "t", "m", "")]
        [TestCase("c.a.e.t.m", "c", "a", "e", "t", "m", null)]
        [TestCase("c.a.e.t.m.s", "c", "a", "e", "t", "m", "s")]
        public void TestComputeRoleId(string roleId, string corp, string app, string env, string tenant, string module,
            string subModules)
        {
            Assert.AreEqual(roleId, RoleUtils.ComputeRoleId(corp, app, env, tenant, module, subModules));
        }
    }
}