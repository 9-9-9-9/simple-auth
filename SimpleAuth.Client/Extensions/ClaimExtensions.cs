using System.Collections.Generic;
using System.Linq;
using SimpleAuth.Client;

namespace System.Security.Claims
{
    public static class ClaimExtensions
    {
        public static Claim GetDefaultSimpleAuthClaim(this IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(x => x.Type == SimpleAuthDefaults.ClaimType);
        }
    }
}