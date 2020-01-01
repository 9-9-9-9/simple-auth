using System;

namespace SimpleAuth.Shared.Enums
{
    [Flags]
    public enum Permission : byte
    {
        None = 0,
        Add = 1,
        View = 2,
        Edit = 4,
        Delete = 8,
        Crud = Add | View | Edit | Delete,
        CurrentMax = Crud,
        Full = 255
    }

    public static class PermissionExtensions
    {
        public static Permission Grant(this Permission permission, params Permission[] permissions)
        {
            var result = permission;
            foreach (var p in permissions)
                result |= p;
            return result;
        }

        public static Permission Revoke(this Permission permission, params Permission[] revokePermissions)
        {
            var result = permission;
            foreach (var p in revokePermissions)
            {
                if (p == Permission.None)
                    continue;
                if (result == Permission.Full)
                    result = Permission.CurrentMax;
                result &= ~p;
            }

            return result;
        }

        public static string Serialize(this Permission permission)
        {
            if (permission == Permission.Full)
                return "*";
            return ((byte) permission).ToString();
        }

        public static Permission Deserialize(this string permission)
        {
            if ("*".Equals(permission))
                return Permission.Full;
            return (Permission) byte.Parse(permission);
        }
    }
}