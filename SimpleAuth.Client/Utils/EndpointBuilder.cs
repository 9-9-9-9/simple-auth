namespace SimpleAuth.Client.Utils
{
    public static class EndpointBuilder
    {
        public static class User
        {
            public static string GetUser(string userId) => $"api/users/{userId}";
            public static string GetActivePermissions(string userId) => $"api/users/{userId}/_permissions";
            public static string CheckPass(string userId) => $"api/users/{userId}/_password";
            public static string CheckGoogleToken() => $"api/external/_google/_token";
            public static string CheckUserPermission(string userId, string roleId, string permission) => $"api/users/{userId}/_permissions/{roleId}/{permission}";
            public static string GetMissingPermissions(string userId) => $"api/users/{userId}/_permissions/_missing";
            public static string AssignUserToPermissionGroups(string userId) => $"api/users/{userId}/_permission-groups";
            public static string UnAssignUserFromAllGroupsAsync(string userId) => $"api/users/{userId}/_permission-groups";
            public const string CreateUser = "api/users";
        }

        public static class Administration
        {
            public static string GenerateCorpPermissionToken(string corp) => $"api/admin/_token/{corp}";
            public static string GenerateAppPermissionToken(string corp, string app) => $"api/admin/_token/{corp}/{app}";
            public static string GenerateAppPermissionToken(string app) => $"api/corp/_token/{app}";
            public static string EncryptPlainText() => "api/admin/_encrypt";
            public static string DecryptData() => "api/admin/_decrypt";
        }

        public static class RoleManagement
        {
            public const string AddRole = "api/roles";
        }

        public static class PermissionGroupManagement
        {
            public const string AddPermissionGroup = "api/permission-groups";
            public static string GetPermissions(string permissionGroupName) => $"api/permission-groups/{permissionGroupName}";
            public static string AddPermissionToGroup(string permissionGroupName) => $"api/permission-groups/{permissionGroupName}/permissions";
            public static string DeletePermissions(string permissionGroupName) => $"api/permission-groups/{permissionGroupName}/permissions";
            public static string UpdateLock(string permissionGroupName) => $"api/permission-groups/{permissionGroupName}/lock";
        }
    }
}