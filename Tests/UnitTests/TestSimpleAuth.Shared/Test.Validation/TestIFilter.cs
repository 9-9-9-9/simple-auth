using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;

namespace Test.SimpleAuth.Shared.Test.Validation
{
    public class TestIFilter
    {
        [TestCase(true, "a")]
        [TestCase(false, " ")]
        [TestCase(true, null)]
        [TestCase(false, "A")]
        [TestCase(false, "-")]
        [TestCase(false, "Aa0")]
        [TestCase(true, "abcdefghijklmnopqrstuvwxyz012345-6789-")]
        [TestCase(false, "abcdefghijklmnopqrstuvwxyz012345 6789-")]
        public void FilterStringIsNullOrNormalized(bool expectedIsValid, string input)
        {
            TestFilter<FilterStringIsNullOrNormalized, string>(expectedIsValid, input);
        }

        [TestCase(true, "a")]
        [TestCase(false, " ")]
        [TestCase(false, null)]
        [TestCase(false, "A")]
        [TestCase(false, "-")]
        [TestCase(false, "Aa0")]
        [TestCase(true, "abcdefghijklmnopqrstuvwxyz012345-6789-")]
        [TestCase(false, "abcdefghijklmnopqrstuvwxyz012345 6789-")]
        public void FilterStringIsNormalizedAndNotNull(bool expectedIsValid, string input)
        {
            TestFilter<FilterStringIsNormalizedAndNotNull, string>(expectedIsValid, input);
        }

        [TestCase(true)]
        [TestCase(true, "a")]
        [TestCase(false, null)]
        [TestCase(false, "A")]
        [TestCase(false, "-")]
        [TestCase(false, "Aa0")]
        [TestCase(true, "abcdefghijklmnopqrstuvwxyz012345-6789-")]
        [TestCase(false, "abcdefghijklmnopqrstuvwxyz012345-6789-", "A")]
        [TestCase(false, "abcdefghijklmnopqrstuvwxyz012345 6789-")]
        [TestCase(false, "a", "a", null)]
        [TestCase(false, "a", null, "A")]
        [TestCase(false, " ", " ", " ")]
        public void FilterCollectionIsNormalizedIfAny(bool expectedIsValid, params string[] args)
        {
            TestFilter<FilterCollectionIsNormalizedIfAny, IEnumerable<string>>(expectedIsValid, args);
        }

        [TestCase(true, "a")]
        [TestCase(false, null)]
        [TestCase(false, "A")]
        [TestCase(false, "-")]
        [TestCase(false, "Aa0")]
        [TestCase(true, "abcdefghijklmnopqrstuvwxyz012345-6789-")]
        [TestCase(false, "abcdefghijklmnopqrstuvwxyz012345 6789-")]
        [TestCase(false, "*")]
        [TestCase(false, "a*")]
        public void RolePartsFilter(bool expectedIsValid, string input)
        {
            TestFilter<FilterCorpInput, string>(expectedIsValid, input);
            TestFilter<FilterAppInput, string>(expectedIsValid, input);
        }

        [TestCase(true, "a")]
        [TestCase(false, null)]
        [TestCase(false, "A")]
        [TestCase(false, "-")]
        [TestCase(false, "Aa0")]
        [TestCase(true, "abcdefghijklmnopqrstuvwxyz012345-6789-")]
        [TestCase(false, "abcdefghijklmnopqrstuvwxyz012345 6789-")]
        [TestCase(true, "*")]
        [TestCase(false, "a*")]
        public void RolePartsAcceptWildCardFilter(bool expectedIsValid, string input)
        {
            TestFilter<FilterEnvInput, string>(expectedIsValid, input);
            TestFilter<FilterTenantInput, string>(expectedIsValid, input);
            TestFilter<FilterModuleInput, string>(expectedIsValid, input);
            TestFilter<FilterSubModulePartInput, string>(expectedIsValid, input);
            TestFilter<FilterArrSubModulesInput, string[]>(expectedIsValid, new[] {input, input});
        }

        [Test]
        public void FilterArrSubModulesInput_AdditionalChecking()
        {
            TestFilter<FilterArrSubModulesInput, string[]>(true, new string[0]); // empty array is an expected result
        }

        [TestCase(true, null)]
        [TestCase(true, "*")]
        [TestCase(false, "-")]
        [TestCase(true, "a|b")]
        [TestCase(true, "a|*")]
        [TestCase(true, "*|b")]
        [TestCase(false, "*|B")]
        [TestCase(false, "A|b")]
        [TestCase(false, "a|B")]
        [TestCase(false, "A|B")]
        [TestCase(false, "a|")]
        [TestCase(false, "|a")]
        [TestCase(false, "a||b")]
        [TestCase(false, "||a|b")]
        [TestCase(true, "a")]
        [TestCase(true, "a|b|c")]
        [TestCase(false, "a|0|-")]
        [TestCase(true, "a|0|*")]
        public void FilterStrSubModulesInput(bool expectedIsValid, string input)
        {
            TestFilter<FilterStrSubModulesInput, string>(expectedIsValid, input);
        }

        [Test]
        public void FilterStrSubModulesInput_AdditionalChecking()
        {
            TestFilter<FilterStrSubModulesInput, string>(false, "!");
            TestFilter<FilterStrSubModulesInput, string>(false, "  ");
            TestFilter<FilterStrSubModulesInput, string>(false, string.Empty);
        }

        [Test]
        public void FilterRoleExistingPartsAreCorrect_1()
        {
            // expect throwing NotSupportedException
            Assert.Catch<NotSupportedException>(() => { new FilterRoleExistingPartsAreCorrect().IsValid(new ICNA()); });
            Assert.Catch<NotSupportedException>(() => { new FilterRoleExistingPartsAreCorrect().IsValid(new NCIA()); });
            Assert.Catch<NotSupportedException>(() => { new FilterRoleExistingPartsAreCorrect().IsValid(new NCNA()); });
            
            // Expect not throwing exception
            new FilterRoleExistingPartsAreCorrect().IsValid(new ICIA());
            new FilterRoleExistingPartsAreCorrect().IsValid(null);
            
            // check when IRawSubModulesRelated then IModuleRelated is required
            TestFilter<FilterRoleExistingPartsAreCorrect, FullParts_UsingRaw>(false, new FullParts_UsingRaw
            {
                Corp = "c",
                App = "a",
                Env = "e",
                Tenant = "t",
                Module = null,
                SubModules = new []{"s"}
            });
        }

        [TestCase(true, "c", "a", "e", "t", "m", "sm")]
        [TestCase(true, "c", "a", "e", "t", "m", null)]
        [TestCase(true, "c", "a", "e", "t", null, null)]
        [TestCase(true, "c", "a", "e", null, null, null)]
        [TestCase(true, "c", "a", null, null, null, null)]
        [TestCase(false, "c", null, null, null, null, null)]
        [TestCase(false, null, null, null, null, null, null)]
        [TestCase(false, null, null, null, null, null, "sm")]
        [TestCase(false, null, null, null, null, "m", "sm")]
        [TestCase(false, null, null, null, "t", "m", "sm")]
        [TestCase(false, null, null, "e", "t", "m", "sm")]
        [TestCase(false, null, "a", "e", "t", "m", "sm")]
        [TestCase(false, "c", "a", "e", "t", null, "sm")]
        [TestCase(false, "c", "a", "e", null, "m", "sm")]
        [TestCase(false, "c", "a", null, "t", "m", "sm")]
        [TestCase(false, "c", null, "e", "t", "m", "sm")]
        [TestCase(false, "c", "a", "e", null, "m", null)]
        [TestCase(false, "c", "a", null, "t", null, "sm")]
        public void FilterRoleExistingPartsAreCorrect_2(bool expectedIsValid, string corp, string app, string env,
            string tenant, string module, string subModules)
        {
            TestFilter<FilterRoleExistingPartsAreCorrect, FullParts>(expectedIsValid, new FullParts
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModules
            });
        }

        [TestCase(true, "a")]
        [TestCase(false, "-")]
        [TestCase(false, null)]
        public void PendingFilter(bool expectedIsValid, string input)
        {
            var pendingFilter = PendingFilter<FilterStringIsNormalizedAndNotNull, string>.New(input);
            Assert.AreEqual(expectedIsValid, pendingFilter.IsValid().IsValid);
        }

        private void TestFilter<TFilter, TInput>(bool expectedIsValid, TInput input)
            where TFilter : IFilter<TInput>, new()
        {
            var vr = new TFilter().IsValid(input);
            vr.Message?.Write();
            Assert.AreEqual(expectedIsValid, vr.IsValid);
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        class ICNA : ICorpRelated
        {
            public string Corp { get; set; }
        }

        class NCIA : IAppRelated
        {
            public string App { get; set; }
        }

        class ICIA : ICorpRelated, IAppRelated
        {
            public string Corp { get; set; }
            public string App { get; set; }
        }

        public class NCNA : IEnvRelated
        {
            public string Env { get; set; }
        }

        class FullParts : ICorpRelated, IAppRelated, IEnvRelated, ITenantRelated, IModuleRelated, ISubModuleRelated
        {
            public string Corp { get; set; }
            public string App { get; set; }
            public string Env { get; set; }
            public string Tenant { get; set; }
            public string Module { get; set; }
            public string SubModules { get; set; }
        }

        class FullParts_UsingRaw : ICorpRelated, IAppRelated, IEnvRelated, ITenantRelated, IModuleRelated, IRawSubModulesRelated
        {
            public string Corp { get; set; }
            public string App { get; set; }
            public string Env { get; set; }
            public string Tenant { get; set; }
            public string Module { get; set; }
            public string[] SubModules { get; set; }
        }
        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming
    }

    public class TestFilterExtensions
    {
        [Test]
        public void Cast()
        {
            // Expect throwing NotSupportedException
            Assert.Catch<NotSupportedException>(() => new FilterRoleExistingPartsAreCorrect().IsValid(new FwT
            {
                Corp = "c",
                App = "a",
                Env = "e",
                Module = "m"
            }));
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        private class FwT : ICorpRelated, IAppRelated, IEnvRelated, IModuleRelated
        {
            public string Corp { get; set; }
            public string App { get; set; }
            public string Env { get; set; }
            public string Module { get; set; }
        }
        
        [TestCase(true, "a", "b")]
        [TestCase(false, "-", "b")]
        [TestCase(false, "a", "-")]
        [TestCase(false, null, null)]
        public void IsValid(bool expectedIsValid, string input1, string input2)
        {
            IPendingFilter pendingFilter1 = PendingFilter<FilterStringIsNormalizedAndNotNull, string>.New(input1);
            IPendingFilter pendingFilter2 = PendingFilter<FilterStringIsNormalizedAndNotNull, string>.New(input2);
            Assert.AreEqual(expectedIsValid, new[] {pendingFilter1, pendingFilter2}.IsValid().IsValid);
        }

        [Test]
        public void IsValid()
        {
            var filter = new FilterStringIsNormalizedAndNotNull();

            Assert.AreEqual(true, new Func<ValidationResult>[]
            {
                () => filter.IsValid("a"),
                () => filter.IsValid("b"),
            }.IsValid().IsValid);
            Assert.AreEqual(false, new Func<ValidationResult>[]
            {
                () => filter.IsValid("-"),
                () => filter.IsValid("b"),
            }.IsValid().IsValid);
            Assert.AreEqual(false, new Func<ValidationResult>[]
            {
                () => filter.IsValid("b"),
                () => filter.IsValid("-"),
            }.IsValid().IsValid);
            Assert.AreEqual(false, new Func<ValidationResult>[]
            {
                () => filter.IsValid("-"),
                () => filter.IsValid("-"),
            }.IsValid().IsValid);
        }

        [Test]
        public void Chain()
        {
            IPendingFilter pendingFilter1 = PendingFilter<FilterStringIsNormalizedAndNotNull, string>.New(null);
            IPendingFilter pendingFilter2 = PendingFilter<FilterStringIsNormalizedAndNotNull, string>.New(null);
            var chained = pendingFilter1.Chain(pendingFilter2);
            Assert.AreEqual(2, chained.Length);
            Assert.AreEqual(pendingFilter1, chained[0]);
            Assert.AreEqual(pendingFilter2, chained[1]);
            IPendingFilter pendingFilter3 = PendingFilter<FilterStringIsNormalizedAndNotNull, string>.New(null);
            chained = pendingFilter3.Chain(chained);
            Assert.AreEqual(3, chained.Length);
            Assert.AreEqual(pendingFilter3, chained[0]);
        }
    }
}