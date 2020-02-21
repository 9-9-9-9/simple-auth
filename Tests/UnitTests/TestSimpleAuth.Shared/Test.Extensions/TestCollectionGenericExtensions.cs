using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleAuth.Shared.Extensions;

namespace Test.SimpleAuth.Shared.Test.Extensions
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
            Assert.AreNotEqual(expected, src.IsEmpty());
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

        [TestCase(1, "a")]
        [TestCase(0, "    ")]
        [TestCase(0, "    ", null)]
        [TestCase(0, " ")]
        [TestCase(0, " ", null)]
        [TestCase(1, "b", null)]
        [TestCase(0)]
        [TestCase(0, null)]
        public void DropBlank(int size, params string[] src)
        {
            Assert.AreEqual(size, src.DropBlank().Count());
        }

        [Test]
        public void Concat()
        {
            List<string> l = null;

            Wipe();
            TestConcat("a", 1);

            Wipe();
            TestConcat(null, 0);

            Init(null, null);
            TestConcat("a", 3);

            Init(null, null);
            TestConcat(null, 0);

            void TestConcat(string element, int expectedSize)
            {
                try
                {
                    var col = l.Concat(element);
                    Assert.NotNull(col);
                    l = col.ToList();
                    Assert.AreEqual(expectedSize, l.Count);
                }
                catch (ArgumentNullException) when (element is null)
                {
                    // OK
                }
                catch
                {
                    Assert.Fail("Error is not expected");
                }
            }

            void Wipe() => l = null;
            void Init(params string[] args) => l = args.ToList();
        }
    }
}