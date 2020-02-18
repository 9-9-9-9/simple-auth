using System;
using System.Linq;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;

namespace SimpleAuth.Client.AspNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SaPermissionAttribute : Attribute
    {
        public Verb Verb { get; }
        public string[] SubModules { get; }

        public SaPermissionAttribute(Verb verb, params string[] subModules)
        {
            if (verb == Verb.None)
                throw new ArgumentException(
                    $"{nameof(verb)}: can't define '{nameof(Verb.None)}' value here");
            
            if (subModules?.Any(x => x == Constants.WildCard) == true)
                throw new ArgumentException($"{nameof(subModules)}: can't use wildcard here");
            
            Verb = verb;
            SubModules = subModules;
        }
    }
}