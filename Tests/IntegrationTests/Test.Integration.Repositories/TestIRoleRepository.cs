using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared;

namespace Test.Integration.Repositories
{
    public class TestIRoleRepository : BaseTestRepo
    {
        [Test]
        public void Search_ValidateParams()
        {
            var repo = Svc<IRoleRepository>();
            Assert.Catch<ArgumentNullException>(() => repo.Search("any", RandomCorp(), null));
            Assert.Catch<ArgumentNullException>(() => repo.Search("any", null, "a1"));
        }

        [TestCase("123", ExpectedResult = 3)]
        [TestCase("", ExpectedResult = 3)]
        [TestCase(null, ExpectedResult = 3)]
        [TestCase("*", ExpectedResult = 3)]
        [TestCase("5", ExpectedResult = 3)]
        [TestCase("ab", ExpectedResult = 3)]
        [TestCase("x", ExpectedResult = 0)]
        [TestCase("678", ExpectedResult = 2)]
        [TestCase("901", ExpectedResult = 2)]
        [TestCase("12345", ExpectedResult = 1)]
        public async Task<int> Search(string term)
        {
            var repo = Svc<IRoleRepository>();
            var corp = Regex.Replace(Guid.NewGuid().ToString(), @"[0-9\-]", "") + "123";
            await InsertRecords();

            var roles = repo.Search(term, corp, "ab").OrEmpty().ToList();

            if (!term.IsBlank() && !Constants.WildCard.Equals(term))
                foreach (var role in roles)
                    Assert.IsTrue(role.Id.Contains(term));

            return roles.Count;

            Task<int> InsertRecords()
            {
                return repo.CreateManyAsync(new[]
                {
                    new Role
                    {
                        Corp = corp,
                        App = "ab",
                        Env = "345",
                        Tenant = "456",
                        Module = "567",
                    }.ComputeId(),
                    new Role
                    {
                        Corp = corp,
                        App = "ab",
                        Env = "4567",
                        Tenant = "6789",
                        Module = "8901"
                    }.ComputeId(),
                    new Role
                    {
                        Corp = corp,
                        App = "ab",
                        Env = "56789",
                        Tenant = "89012",
                        Module = "12345"
                    }.ComputeId(),
                });
            }
        }

        [Test]
        public void DeleteManyAsync()
        {
            var repo = Svc<IRoleRepository>();
            Assert.CatchAsync<NotSupportedException>(async () => await repo.DeleteManyAsync(null));
        }

        [Flags]
        public enum Parts : byte
        {
            None = 0,
            C = 1,
            A = 2,
            E = 4,
            T = 8,
            M = 16
        }
    }
}