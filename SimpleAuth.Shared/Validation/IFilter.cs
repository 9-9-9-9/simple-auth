using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Models;
using ValidationResult = SimpleAuth.Shared.Models.ValidationResult;

namespace SimpleAuth.Shared.Validation
{
    public interface IFilter<in T>
    {
        ValidationResult IsValid(T src);
    }

    public abstract class Filter<T> : IFilter<T>
    {
        protected abstract bool AcceptNull { get; }

        public virtual ValidationResult IsValid(T src)
        {
            if (src is null)
            {
                if (AcceptNull)
                    return ValidationResult.Valid();
                else
                    return ValidationResult.Invalid($"{nameof(src)} is null");
            }

            return CheckValidInternal(src);
        }

        protected abstract ValidationResult CheckValidInternal(T src);
    }

    public abstract class StringFilter : Filter<string>
    {
        protected virtual bool AcceptBlankString => false;
        protected virtual bool AcceptEmptyString => false;

        protected virtual ValidationResult IsValidString(string src)
        {
            if (src.IsEmpty() && !AcceptEmptyString)
                return ValidationResult.Invalid($"{nameof(src)} is empty string");

            if (src.IsBlank() && !AcceptBlankString)
                return ValidationResult.Invalid($"{nameof(src)} is blank");

            return ValidationResult.Valid();
        }

        protected virtual ValidationResult IsNormalized(string src)
        {
            foreach (var c in src.ToCharArray())
            {
                if (c >= 'a' && c <= 'z')
                    continue;
                if (c >= '0' && c <= '9')
                    continue;
                if (c == '-')
                    continue;
                return ValidationResult.Invalid("String is not normalized");
            }

            if (src.Length == 1 && src[0] == '-')
                return ValidationResult.Invalid("Hyphen '-' is not allowed to standing alone");

            return ValidationResult.Valid();
        }
    }

    public class FilterStringIsNullOrNormalized : StringFilter
    {
        protected override bool AcceptNull => true;

        protected override ValidationResult CheckValidInternal(string src)
        {
            return IsNormalized(src);
        }
    }

    public class FilterStringIsNormalizedAndNotNull : StringFilter
    {
        protected override bool AcceptNull => false;

        protected override ValidationResult CheckValidInternal(string src)
        {
            if (src.Trim().Length == 0)
                return ValidationResult.Invalid("Blank is not accepted");
            return IsNormalized(src);
        }
    }

    public class FilterCollectionIsNormalizedIfAny : IFilter<IEnumerable<string>>
    {
        public ValidationResult IsValid(IEnumerable<string> src)
        {
            try
            {
                var filter = new FilterStringIsNormalizedAndNotNull();
                foreach (var part in src)
                {
                    var vr = filter.IsValid(part);
                    if (!vr.IsValid)
                        return vr;
                }

                return ValidationResult.Valid();
            }
            catch (NullReferenceException)
            {
                return ValidationResult.Valid();
            }
        }
    }

    public abstract class RolePartsFilter : StringFilter
    {
        protected override bool AcceptNull => false;

        protected override ValidationResult CheckValidInternal(string src)
        {
            var vr = IsValidString(src);
            if (!vr.IsValid)
                return vr;

            return new FilterStringIsNullOrNormalized().IsValid(src);
        }
    }

    public class FilterCorpInput : RolePartsFilter
    {
    }

    public class FilterAppInput : RolePartsFilter
    {
    }

    public abstract class RolePartsAcceptWildCardFilter : StringFilter
    {
        protected override bool AcceptNull => false;

        protected override ValidationResult CheckValidInternal(string src)
        {
            var vr = IsValidString(src);
            if (!vr.IsValid)
                return vr;

            if (src == "*")
                return ValidationResult.Valid();

            return new FilterStringIsNullOrNormalized().IsValid(src);
        }
    }

    public class FilterEnvInput : RolePartsAcceptWildCardFilter
    {
    }

    public class FilterTenantInput : RolePartsAcceptWildCardFilter
    {
    }

    public class FilterModuleInput : RolePartsAcceptWildCardFilter
    {
    }

    public class FilterArrSubModulesInput : Filter<string[]>
    {
        protected override bool AcceptNull => true;

        protected override ValidationResult CheckValidInternal(string[] src)
        {
            if (src.Length == 0)
                return ValidationResult.Valid();

            foreach (var part in src)
            {
                var vr = new FilterSubModulePartInput().IsValid(part);
                if (!vr.IsValid)
                    return vr;
            }

            return ValidationResult.Valid();
        }
    }

    public class FilterSubModulePartInput : RolePartsFilter
    {
    }

    public class FilterStrSubModulesInput : StringFilter
    {
        protected override bool AcceptNull => true;

        protected override bool AcceptBlankString => false;

        protected override bool AcceptEmptyString => false;

        protected override ValidationResult CheckValidInternal(string src)
        {
            var vr = IsValidString(src);
            if (!vr.IsValid)
                return vr;

            foreach (var part in src.Split(Constants.ChSplitterSubModules))
            {
                vr = new FilterSubModulePartInput().IsValid(part);
                if (!vr.IsValid)
                    return vr;
            }

            return ValidationResult.Valid();
        }
    }

    public class FilterRoleExistingPartsAreCorrect : Filter<IRolePart>
    {
        protected override bool AcceptNull => false;

        protected override ValidationResult CheckValidInternal(IRolePart src)
        {
            if (!(src is ICorpRelated) || !(src is IAppRelated))
                throw new NotSupportedException(src.GetType().FullName);

            if (((ICorpRelated) src).Corp.IsBlank())
                return ValidationResult.Invalid($"{nameof(ICorpRelated.Corp)} is required");

            if (((IAppRelated) src).App.IsBlank())
                return ValidationResult.Invalid($"{nameof(IAppRelated.App)} is required");

            if (src is ISubModuleRelated smr && !smr.SubModules.IsBlank())
            {
                if (src.Cast<IModuleRelated>().Module.IsBlank())
                    return ValidationResult.Invalid($"{nameof(IModuleRelated.Module)} is required");
            }

            if (src is IRawSubModulesRelated rsr && rsr.SubModules.Any())
            {
                if (src.Cast<IModuleRelated>().Module.IsBlank())
                    return ValidationResult.Invalid($"{nameof(IModuleRelated.Module)} is required");
            }

            if (src is IModuleRelated mr && !mr.Module.IsBlank())
            {
                if (src.Cast<ITenantRelated>().Tenant.IsBlank())
                    return ValidationResult.Invalid($"{nameof(ITenantRelated.Tenant)} is required");
            }

            if (src is ITenantRelated tr && !tr.Tenant.IsBlank())
            {
                if (src.Cast<IEnvRelated>().Env.IsBlank())
                    return ValidationResult.Invalid($"{nameof(IEnvRelated.Env)} is required");
            }

            /*
            if (src is IEnvRelated er && !er.Env.IsBlank())
            {
                // App and Corp Never Blank
            }
            */

            return ValidationResult.Valid();
        }
    }

    public class FilterGoodPassword : StringFilter
    {
        protected override bool AcceptNull => true;

        private readonly char[] _rejectedChars = new[] {'\t', '\n', '\r'};
        
        protected override ValidationResult CheckValidInternal(string src)
        {
            if (src.Length != src.Trim().Length)
                return ValidationResult.Invalid("Can not starts or ends with spaces");
            if (src.Length < 10)
                return ValidationResult.Invalid("Require at least 10 characters");
            if (!Regex.IsMatch(src, ".*[A-Z].*"))
                return ValidationResult.Invalid("Require at least 1 UPPER CASE character");
            if (!Regex.IsMatch(src, ".*[a-z].*"))
                return ValidationResult.Invalid("Require at least 1 lower case character");
            if (!Regex.IsMatch(src, ".*[0-9].*"))
                return ValidationResult.Invalid("Require at least 1 digit (number)");
            if (!Regex.IsMatch(src, ".*[^\\w].*"))
                return ValidationResult.Invalid("Require at least 1 non alpha-numeric character");
            var charsArray = src.ToCharArray(); 
            if (_rejectedChars.Any(rjc => charsArray.Contains(rjc)))
                return ValidationResult.Invalid("\\t, \\n, \\r characters are not allowed in password");
            
            return ValidationResult.Valid();
        }
    }

    public class FilterGoodUserId : StringFilter
    {
        protected override bool AcceptNull => false;

        private readonly char[] _acceptedChars = new[] {'-', '_', '.', '@', '+'};

        protected override ValidationResult CheckValidInternal(string src)
        {
            if (src.Length < 6)
                return ValidationResult.Invalid("Require at least 6 characters");
            
            var vr = IsValidString(src);
            if (!vr.IsValid)
                return vr;

            foreach (var c in src.ToCharArray())
            {
                if (c >= 'a' && c <= 'z')
                    continue;
                if (c >= '0' && c <= '9')
                    continue;
                if (_acceptedChars.Contains(c))
                    continue;
                return ValidationResult.Invalid(
                    "User Id only accepts the following lower case alpha numeric characters, and some special characters like: '-', '_', '.', '@', '+'"
                );
            }

            return ValidationResult.Valid();
        }
    }

    public class FilterEmail : StringFilter
    {
        protected override bool AcceptNull => true;
        protected override ValidationResult CheckValidInternal(string src)
        {
            try
            {
                if (src.Trim() != src || src.Contains(' ') || src.Contains('\t'))
                    return ValidationResult.Invalid("Email contains spaces");
                if (!new EmailAddressAttribute().IsValid(src))
                    return ValidationResult.Invalid("Wrong email format");
                return ValidationResult.Valid();
            }
            catch
            {
                return ValidationResult.Invalid("Wrong email format");
            }
        }
    }

    public interface IPendingFilter
    {
        ValidationResult IsValid();
    }

    public class PendingFilter<TFilter, TInput> : IPendingFilter
        where TFilter : IFilter<TInput>, new()
    {
        private readonly TFilter _filter;
        private readonly TInput _input;

        public PendingFilter(TFilter filter, TInput input)
        {
            _filter = filter;
            _input = input;
        }

        public ValidationResult IsValid()
        {
            return _filter.IsValid(_input);
        }

        public static PendingFilter<TFilter, TInput> New(TInput input)
        {
            return new PendingFilter<TFilter, TInput>(new TFilter(), input);
        }
    }

    public static class FilterExtensions
    {
        internal static TTo Cast<TTo>(this IRolePart fromObj) where TTo : IRolePart
        {
            if (fromObj is TTo toObj)
                return toObj;
            throw new NotSupportedException($"{fromObj.GetType().Name} must implements {typeof(TTo).Name}");
        }

        public static ValidationResult IsValid(this IEnumerable<IPendingFilter> pendingFilters)
        {
            foreach (var pendingFilter in pendingFilters)
            {
                var vr = pendingFilter.IsValid();
                if (!vr.IsValid)
                    return vr;
            }

            return ValidationResult.Valid();
        }

        public static ValidationResult IsValid(this IEnumerable<Func<ValidationResult>> pendingValidatorActions)
        {
            foreach (var resultFactory in pendingValidatorActions)
            {
                var vr = resultFactory();
                if (!vr.IsValid)
                    return vr;
            }
            return ValidationResult.Valid();
        }

        public static IPendingFilter[] Chain(this IPendingFilter pendingFilter,
            params IPendingFilter[] morePendingFilters)
        {
            var list = new List<IPendingFilter>
            {
                pendingFilter
            };
            list.AddRange(morePendingFilters);
            return list.ToArray();
        }
    }
}