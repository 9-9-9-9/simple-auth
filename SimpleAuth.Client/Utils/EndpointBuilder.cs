using System.Net;

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

        public static class Administration
        {
            public static string GenerateCorpPermissionToken(string corp) => $"admin/token/{corp}";
            public static string GenerateAppPermissionToken(string corp, string app) => $"admin/token/{corp}/{app}";
            public static string GenerateAppPermissionToken(string app) => $"corp/token/{app}";
            public static string EncryptPlainText() => "admin/encrypt";
            public static string DecryptData() => "admin/decrypt";
        }
    }
}