using System;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SaModuleAttribute : Attribute
    {
        public string Module { get; }
        
        public SaModuleAttribute(string module)
        {
            Module = module;
        }
    }
}