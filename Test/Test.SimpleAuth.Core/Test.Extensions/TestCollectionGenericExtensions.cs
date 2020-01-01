using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;

namespace Test.SimpleAuth.Core.Test.Extensions
{
    public class TestCollectionGenericExtensions
    {
        [TestCase(true, "a")]
        [TestCase(true, 1)]
        [TestCase(true, 1, null)]
        [TestCase(true, null, "a")]
        [TestCase(true, null)]
        [TestCase(false)]
        public void IsAny(bool expected, params object[] src)
        {
            Assert.AreEqual(expected, src.IsAny());
        }

        [Test]
        public void IsAny()
        {
            Assert.IsFalse((new List<object>()).IsAny());
            Assert.IsFalse(((string[]) null).IsAny());
        }

        [TestCase(1, "a")]
        [TestCase(1, 1)]
        [TestCase(2, 1, null)]
        [TestCase(0)]
        public void OrEmpty(int size, params object[] src)
        {
            Assert.AreEqual(size, src.OrEmpty().Count());
        }

        [Test]
        public void OrEmpty()
        {
            Assert.AreEqual(0, (new List<object>()).OrEmpty().Count());
            Assert.AreEqual(0, ((string[]) null).OrEmpty().Count());
        }
        
        [TestCase(1, "a")]
        [TestCase(1, 1)]
        [TestCase(1, 1, null)]
        [TestCase(0)]
        [TestCase(0, null)]
        public void DropNull(int size, params object[] src)
        {
            Assert.AreEqual(size, src.DropNull().Count());
        }
    }
}