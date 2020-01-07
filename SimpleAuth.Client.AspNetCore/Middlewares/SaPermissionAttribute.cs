using System;
using SimpleAuth.Shared.Enums;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SaPermissionAttribute : Attribute
    {
        public Permission Permission { get; }
        public string[] SubModules { get; }

        public SaPermissionAttribute(Permission permission, params string[] subModules)
        {
            Permission = permission;
            SubModules = subModules;
        }
    }
}