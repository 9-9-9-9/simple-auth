using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;

namespace Test.SimpleAuth.Shared.Test.Repositories
{
    public class TestConcurrentUpdatePrevention : BaseTestClass
    {
        [Test]
        public async Task TestRowVersioning()
        {
            var corp = RandomCorp();
            var groupId = Guid.NewGuid();
            var roleGroupRepository = Svc<IRoleGroupRepository>();
            await roleGroupRepository.CreateAsync(new RoleGroup
            {
                Id = groupId,
                Corp = corp,
                App = "a",
                Name = "g",
            });

            var g1 = roleGroupRepository.Find(groupId);
            var g2 = roleGroupRepository.Find(groupId);
            
            Assert.NotNull(g1);
            Assert.NotNull(g2);
            Assert.IsNull(g1.RowVersion);
            Assert.IsNull(g2.RowVersion);
            Assert.IsTrue(g1 != g2);

            g1.Name = "g1";
            await roleGroupRepository.UpdateAsync(g1); 
            g1 = roleGroupRepository.Find(groupId);
            
            Assert.AreEqual("g1", g1.Name);
            Assert.AreEqual(1, g1.RowVersion);

            g2.Name = "g2";

            try
            {
                await roleGroupRepository.UpdateAsync(g2);
                Assert.Fail("Expect exception");
            }
            catch (Exception e)
            {
                // OK
            }
        }
    }
}