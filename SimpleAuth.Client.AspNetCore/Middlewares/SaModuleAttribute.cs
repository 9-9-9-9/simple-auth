using System;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class SaModuleAttribute : Attribute
    {
        public string Module { get; }
        public bool Restricted { get; }
        
        public SaModuleAttribute(string module, bool restricted = true)
        {
            Module = module;
            Restricted = restricted;
        }
    }
}