using System;

namespace SimpleAuth.Client.Extensions.PermissionGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SaTenantAttribute : Attribute
    {
        public string Tenant { get; }

        public SaTenantAttribute(string tenant)
        {
            Tenant = tenant;
        }
    }
}