using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using SimpleAuth.Shared.Models;

namespace Test.SimpleAuth.Shared.Test.Models
{
    public class TestResponseUserModel
    {
        [Test]
        public void ExpireAt()
        {
            var rum = new ResponseUserModel();
            Assert.IsNull(rum.ExpiryDate);

            Assert.Catch<InvalidDataException>(() => rum.ExpireAt(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local)));

            var offset = 50000000;
            var epochZeroToOffset = new DateTime(1970, 1, 1, 1, 1, 1, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(offset));
            
            rum.ExpireAt(epochZeroToOffset);
            
            Assert.NotNull(rum.ExpiryDate);
            Assert.AreEqual(offset, rum.ExpiryDate);
        }
    }
}