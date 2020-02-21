using System;
using System.Linq;
using NUnit.Framework;
using SimpleAuth.Shared.IO;

namespace Test.SimpleAuth.Core.Test.IO
{
    public class TestTmlFile
    {
        [Test]
        public void Test_1()
        {
            var lines = Src1.Replace(' ', '\t').Split('\r', '\n');
            Assert.AreEqual(12, lines.Length);
            
            var root = TmlFile.Parse(lines);
            Assert.NotNull(root);
            
            Assert.AreEqual(1, root.ChildrenNodes.Count);

            var abc = root.ChildrenNodes.Single();
            Assert.NotNull(abc);
            Assert.AreEqual("abc", abc.Content);
            
            Assert.AreEqual(3, abc.ChildrenNodes.Count);

            var def = abc.SingleOrDefault(x => x.Content == "def");
            Assert.NotNull(def);
            Assert.IsTrue(def.HasChildNode);
            Assert.AreEqual(2, def.Where(x => true).Count());
            Assert.AreEqual(1, def.ChildrenNodes.Count(x => x.Content == "ghi"));
            Assert.AreEqual(1, def.ChildrenNodes.Count(x => x.Content == "klm"));
            
            var nop = abc.SingleOrDefault(x => x.Content == "nop");
            Assert.NotNull(nop);
            Assert.IsFalse(nop.HasChildNode);
            
            var qrs = abc.SingleOrDefault(x => x.Content == "qrs");
            Assert.NotNull(qrs);
            Assert.AreEqual(1, qrs.ChildrenNodes.Count);

            var tuv = qrs.SingleOrDefault(x => x.Content == "tuv");
            Assert.NotNull(tuv);
            Assert.AreEqual(1, tuv.ChildrenNodes.Count);

            var wxyz = tuv.SingleOrDefault(x => x.Content == "wxyz");
            Assert.NotNull(wxyz);
            Assert.IsFalse(wxyz.HasChildNode);
            
            Assert.IsEmpty(tuv.Where(x => x.Content == "0"));
        }

        [Test]
        public void Test_2_3()
        {
            Assert.Catch<ArgumentException>(() => TmlFile.Parse(Src2.Replace(' ', '\t').Split('\r', '\n')));
            Assert.Catch<ArgumentException>(() => TmlFile.Parse(Src3.Replace(' ', '\t').Split('\r', '\n')));
        }

        private const string Src1 = @"
abc
 def
  ghi
  klm

 nop

 qrs
  tuv
   wxyz
";

        private const string Src2 = @"
 abc
 def
  ghi
  klm
";
        private const string Src3 = @"
abc
 def
   ghi
  klm
";
    }
}