namespace SimpleAuth.Client.Utils
{
    public static class EndpointBuilder
    {
        public static class User
        {
            public static string GetActiveRoles(string userId) => $"api/users/{userId}/roles";
            public static string CheckPass(string userId) => $"api/users/{userId}/password";
            public static string CheckGoogleToken(string userId) => $"api/users/{userId}/roles";
            public static string CheckUserPermission(string userId, string roleId, string permission) => $"api/users/{userId}/roles/{roleId}/{permission}";
            public static string AssignUserToRoleGroups(string userId) => $"api/users/{userId}/role-groups";
            public const string CreateUser = "api/users";
        }

        public static class Administration
        {
            public static string GenerateCorpPermissionToken(string corp) => $"admin/token/{corp}";
            public static string GenerateAppPermissionToken(string corp, string app) => $"admin/token/{corp}/{app}";
            public static string GenerateAppPermissionToken(string app) => $"corp/token/{app}";
            public static string EncryptPlainText() => "admin/encrypt";
            public static string DecryptData() => "admin/decrypt";
        }

        public static class RoleManagement
        {
            public const string AddRole = "api/roles";
        }

        public static class RoleGroupManagement
        {
            public const string AddRoleGroup = "api/role-groups";
            public static string GetRoles(string roleGroupName) => $"api/role-groups/{roleGroupName}";
            public static string AddRoleToGroup(string roleGroupName) => $"api/role-groups/{roleGroupName}/roles";
            public static string DeleteRoles(string roleGroupName) => $"api/role-groups/{roleGroupName}/roles";
            public static string UpdateLock(string roleGroupName) => $"api/role-groups/{roleGroupName}/lock";
        }
    }
}