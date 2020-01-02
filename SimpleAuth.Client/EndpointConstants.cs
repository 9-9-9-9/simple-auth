namespace SimpleAuth.Client
{
    public static class EndpointConstants
    {
        public static class User
        {
            public static string GetActiveRoles(string userId) => $"api/users/{userId}/roles";
            public static string CheckPass(string userId) => $"api/users/{userId}/password";
            public static string CheckGoogleToken(string userId) => $"api/users/{userId}/roles";
        }
    }
}