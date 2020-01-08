using SimpleAuth.Client.Models;

namespace SimpleAuth.Client
{
    public static class SimpleAuthDefaults
    {
        public const string AuthenticationScheme = nameof(SimpleAuth);
        public const string ClaimType = nameof(SimpleAuthorizationClaim);
    }
}