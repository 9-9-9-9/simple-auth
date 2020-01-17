using SimpleAuth.Client.Models;
using SimpleAuth.Shared;

namespace SimpleAuth.Client
{
    public static class SimpleAuthDefaults
    {
        public const string ClaimType = nameof(SimpleAuthorizationClaim);
        public const string ClaimValueType = nameof(SimpleAuthorizationClaim);
        public const string ClaimIssuer = Constants.Identity.Issuer;
    }
}