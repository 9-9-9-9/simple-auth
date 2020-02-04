using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Validation;
using Test.Shared;

namespace Test.SimpleAuth.Shared.Test.Validation
{
    public class TestIRolePartsValidationService : BaseTestClass
    {
        [TestCase(true, "a")]
        [TestCase(false, null)]
        [TestCase(false, "A")]
        [TestCase(false, "-")]
        [TestCase(false, "Aa0")]
        [TestCase(true, "abcdefghijklmnopqrstuvwxyz012345-6789-")]
        [TestCase(false, "abcdefghijklmnopqrstuvwxyz012345 6789-")]
        [TestCase(false, "*")]
        [TestCase(false, "a*")]
        // ReSharper disable once InconsistentNaming
        public void IRolePartsValidationService(bool expectedIsValid, string input)
        {
            var svc = Svc<IRolePartsValidationService>();
            Assert.AreEqual(expectedIsValid, svc.IsValidCorp(input).IsValid);
            Assert.AreEqual(expectedIsValid, svc.IsValidApp(input).IsValid);
        }
    }
}