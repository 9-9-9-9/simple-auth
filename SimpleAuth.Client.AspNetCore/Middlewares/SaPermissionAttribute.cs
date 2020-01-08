using System;
using System.Linq;
using SimpleAuth.Shared;
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
            if (permission == Permission.None)
                throw new ArgumentException(
                    $"{nameof(permission)}: can't define '{nameof(Permission.None)}' value here");
            if (subModules?.Any(x => x == Constants.WildCard) == true)
                throw new ArgumentException($"{nameof(subModules)}: can't use wildcard here");
            Permission = permission;
            SubModules = subModules;
        }
    }
}