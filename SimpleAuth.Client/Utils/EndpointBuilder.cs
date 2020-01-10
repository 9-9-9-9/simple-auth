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
        }
    }
}