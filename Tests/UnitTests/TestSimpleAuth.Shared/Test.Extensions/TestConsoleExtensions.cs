using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleAuth.Shared.Extensions;

namespace Test.SimpleAuth.Shared.Test.Extensions
{
    public class TestConsoleExtensions
    {
        [Test]
        public void Write()
        {
            // Make sure not throwing exception when null
            ((string)null).Write();
            ((ICollection<Int32>)null).Write();
        }
    }
}