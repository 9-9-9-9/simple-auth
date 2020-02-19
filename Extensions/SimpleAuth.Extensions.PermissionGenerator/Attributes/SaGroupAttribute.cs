using System;

namespace SimpleAuth.Extensions.PermissionGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SaGroupAttribute : Attribute
    {
        public string Name { get; }

        public SaGroupAttribute(string name)
        {
            Name = name;
        }
    }
}