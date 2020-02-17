using NUnit.Framework;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;
using Test.Shared;

namespace Test.SimpleAuth.Shared.Test.Validation
{
    public class TestIPermissionGroupValidationService : BaseTestClass
    {
        [TestCase("n", "c", "a", null, ExpectedResult = true)]
        [TestCase("n", "c", "a", "a'b-b", ExpectedResult = true)]
        [TestCase("n", "c", "a", "", ExpectedResult = false)]
        [TestCase("n", "c", "*", "b'c", ExpectedResult = false)]
        [TestCase("n", "*", "a", null, ExpectedResult = false)]
        [TestCase("*", "c", "a", null, ExpectedResult = false)]
        [TestCase("n", "c", null, "a", ExpectedResult = false)]
        [TestCase("n", null, "a", "a", ExpectedResult = false)]
        [TestCase(null, "c", "a", "", ExpectedResult = false)]
        [TestCase("N", "c", "a", null, ExpectedResult = false)]
        [TestCase("n", "C", "a", null, ExpectedResult = false)]
        [TestCase("n", "c", "A", null, ExpectedResult = false)]
        public bool IsValidCreatePermissionGroupModel(string name, string corp, string app, string theGroupsToCopyFrom)
        {
            string[] copyFrom = null;
            if (theGroupsToCopyFrom != null)
            {
                copyFrom = theGroupsToCopyFrom.Split('\'');
            }
            //CreatePermissionGroupModel
            return Svc<IPermissionGroupValidationService>().IsValid(new CreatePermissionGroupModel
            {
                Name = name,
                Corp = corp,
                App = app,
                CopyFromPermissionGroups = copyFrom
            }).IsValid;
        }
    }
}