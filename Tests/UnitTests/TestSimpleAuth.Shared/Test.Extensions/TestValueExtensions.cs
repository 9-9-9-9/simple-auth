using System;
using NUnit.Framework;
using SimpleAuth.Shared.Extensions;

namespace Test.SimpleAuth.Shared.Test.Extensions
{
    public class TestValueExtensions
    {
        [TestCase("a", "a", "b")]
        [TestCase("b", "b", "a")]
        [TestCase("", null, "")]
        [TestCase("a", null, "a")]
        [TestCase("a", "a", null)]
        [TestCase(null, null, null)]
        public void Or_String(string expect, string left, string right)
        {
            Assert.AreEqual(expect, left.Or(right));
        }

        [Test]
        public void Or_Class()
        {
            var obj1 = new Exception();
            var obj2 = new ArgumentException();
            Exception obj3 = null;
            // ReSharper disable ExpressionIsAlwaysNull
            Assert.AreEqual(obj1, obj1.Or(obj2));
            Assert.AreEqual(obj2, obj2.Or(obj1));
            Assert.AreEqual(obj1, obj1.Or(obj3));
            Assert.AreEqual(obj1, obj3.Or(obj1));
            Assert.AreEqual(obj2, obj3.Or(obj2));
            // ReSharper restore ExpressionIsAlwaysNull
        }

        [Test]
        public void OrStruct()
        {
            DateTime dtDef = default;
            var now = DateTime.Now;
            
            Assert.AreEqual(now, now.OrStruct(dtDef));
            Assert.AreEqual(now, dtDef.OrStruct(now));
            Assert.AreEqual(dtDef, dtDef.OrStruct(dtDef));

            var utcNow = DateTime.UtcNow;
            Assert.AreEqual(now, now.OrStruct(utcNow));
            Assert.AreEqual(utcNow, utcNow.OrStruct(now));
        }

        [TestCase("a", "A")]
        [TestCase("a", "a")]
        [TestCase(null, null)]
        [TestCase("á", "Á")]
        public void NormalizeInput(string expected, string src)
        {
            Assert.AreEqual(expected, src.NormalizeInput());
        }
        
        [TestCase(true, "")]
        [TestCase(true, null)]
        [TestCase(true, "        ")]
        [TestCase(true, " ")]
        [TestCase(false, "a")]
        [TestCase(false, " a")]
        [TestCase(false, "a    ")]
        public void IsBlank(bool expected, string src)
        {
            Assert.AreEqual(expected, src.IsBlank());
        }
        
        [TestCase(true, "")]
        [TestCase(true, null)]
        [TestCase(false, "        ")]
        [TestCase(false, " ")]
        [TestCase(false, "a")]
        [TestCase(false, " a")]
        [TestCase(false, "a    ")]
        public void IsEmpty(bool expected, string src)
        {
            Assert.AreEqual(expected, src.IsEmpty());
        }

        [TestCase(true, "a", "A")]
        [TestCase(true, "á", "Á")]
        [TestCase(false, "a", "b")]
        public void EqualsIgnoreCase(bool expected, string left, string right)
        {
            Assert.AreEqual(expected, left.EqualsIgnoreCase(right));
        }

        [TestCase(null, "")]
        [TestCase(null, " ")]
        [TestCase(null, "        ")]
        [TestCase("a", "    a")]
        [TestCase("a", "a    ")]
        public void TrimToNull(string expect, string src)
        {
            Assert.AreEqual(expect, src.TrimToNull());
        }
    }
}