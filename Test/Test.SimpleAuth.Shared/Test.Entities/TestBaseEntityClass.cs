using System;
using NUnit.Framework;
using SimpleAuth.Services.Entities;

namespace Test.SimpleAuth.Shared.Test.Entities
{
    public class TestBaseEntityClass
    {
        [Test]
        public void BaseEntityExtensions_WithRandomId()
        {
            var roleGroup = new RoleGroup();
            Assert.AreEqual(Guid.Empty, roleGroup.Id);
            roleGroup.WithRandomId();
            Assert.AreNotEqual(Guid.Empty, roleGroup.Id);
            try
            {
                roleGroup.WithRandomId();
                Assert.Fail("Error expected");
            }
            catch (InvalidOperationException)
            {
                // OK
            }
        }
    }
}