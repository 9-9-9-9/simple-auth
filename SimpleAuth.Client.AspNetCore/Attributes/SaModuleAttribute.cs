using System;
using SimpleAuth.Shared;

namespace SimpleAuth.Client.AspNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class SaModuleAttribute : Attribute
    {
        public string Module { get; }
        public bool Restricted { get; }
        
        public SaModuleAttribute(string module, bool restricted = true)
        {
            if (module == Constants.WildCard)
                throw new ArgumentException($"{nameof(module)}: can't use wildcard here");
            Module = module;
            Restricted = restricted;
        }
    }
}