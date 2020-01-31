using NUnit.Framework;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;
using Test.Shared;

namespace Test.SimpleAuth.Shared.Services.Validation
{
    public class TestIRoleValidationService : BaseTestClass
    {
        [TestCase("c", "a", "e", "t", "m", "1'2", ExpectedResult = true)]
        [TestCase("c", "a", "e", "t", "m", null, ExpectedResult = true)]
        [TestCase("", "a", "e", "t", "m", "1'2", ExpectedResult = false)]
        [TestCase("c", "", "e", "t", "m", "1'2", ExpectedResult = false)]
        [TestCase("c", "a", "", "t", "m", "1'2", ExpectedResult = false)]
        [TestCase("c", "a", "e", "", "m", "1'2", ExpectedResult = false)]
        [TestCase("c", "a", "e", "t", "", "1'2", ExpectedResult = false)]
        [TestCase("c", "a", "e", "t", "m", "", ExpectedResult = false)]
        [TestCase("c", "a", "", "", "", null, ExpectedResult = false)]
        [TestCase("c", "a", "e", "t", "m", "1", ExpectedResult = true)]
        [TestCase("c", "a", "e", "t", "m", "1|", ExpectedResult = false)]
        [TestCase("c", "a", "e", "t", "m", "|1", ExpectedResult = false)]
        [TestCase("c", "a", "e", "t", "m", "'1", ExpectedResult = false)]
        [TestCase("c", "a", "e", "t", "m", " '1", ExpectedResult = false)]
        [TestCase("c", "a", "e", "t", "m", "*", ExpectedResult = true)]
        [TestCase("*", "a", "e", "t", "m", null, ExpectedResult = false)] // "No wildcard as Corp mtf"
        [TestCase("c", "*", "e", "t", "m", null, ExpectedResult = false)] // "No wildcard as App mtf"
        [TestCase("c", "a", "*", "t", "m", null, ExpectedResult = true)]
        [TestCase("c", "a", "e", "*", "m", null, ExpectedResult = true)]
        [TestCase("c", "a", "e", "t", "*", null, ExpectedResult = true)]
        [TestCase("c", "a", "e", "t", "*", "1", ExpectedResult = true)]
        public bool IsValidCreateRoleModel(string corp, string app, string env, string tenant, string module,
            string theSubmodules)
        {
            string[] subModules = null;
            if (theSubmodules != null)
            {
                subModules = theSubmodules.Split('\'');
            }

            return Svc<IRoleValidationService>().IsValid(new CreateRoleModel
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModules
            }).IsValid;
        }
    }
}