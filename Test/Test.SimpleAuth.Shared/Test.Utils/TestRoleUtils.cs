using System;
using System.Linq;
using NUnit.Framework;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Utils;

namespace Test.SimpleAuth.Shared.Test.Utils
{
    public class TestRoleUtils
    {
        private const string Splitor = "`";

        [TestCase(2, "0`1", "*.*.*.*.*`View", "*.*.*.*.*`Edit")]
        [TestCase(1, "0", "*.*.*.*.*`Full", "*.*.*.*.b`View")]
        [TestCase(1, "1", "*.*.*.*.b`View", "*.*.*.*.*`Full")]
        [TestCase(1, "1", "*.*.*.*.b`View", "*.*.*.*.*`Full", "*.*.*.*.a`View", "*.*.*.*.b`Edit")]
        public void TestDistinct(int expectedNumberOfRolesRemaining, string expectRemainingIndexes,
            params string[] roles)
        {
            var clientRoleModels = roles
                .Select(x => x.Split("`"))
                .Select(x => (x[0], (Permission) Enum.Parse(typeof(Permission), x[1])))
                .Select(x =>
                {
                    RoleUtils.Parse(x.Item1, out var clientRoleModel);
                    clientRoleModel.Permission = x.Item2;
                    return clientRoleModel;
                }).ToArray();

            clientRoleModels = RoleUtils.Distinct(clientRoleModels).ToArray();

            Assert.AreEqual(expectedNumberOfRolesRemaining, clientRoleModels.Length);

            var remaining = clientRoleModels.Select(x => $"{x.ComputeId()}{Splitor}{x.Permission}").ToList();
            foreach (var idx in expectRemainingIndexes.Split(Splitor).Select(int.Parse))
                Assert.IsTrue(remaining.Contains(roles[idx]));
        }
    }
}