using System;
using System.Collections.Generic;
using System.Linq;
using SimpleAuth.Client.Models;

namespace SimpleAuth.Client.Utils
{
    public static class AuthorizationUtils
    {
        public static ICollection<SimpleAuthorizationClaim> GetMissingClaims(
            IEnumerable<SimpleAuthorizationClaim> existingClaims,
            IEnumerable<SimpleAuthorizationClaim> requiredClaims)
        {
            if (requiredClaims == null)
                throw new ArgumentNullException(nameof(requiredClaims));
            
            if (existingClaims == null)
                return requiredClaims.ToArray(); // missing all

            return requiredClaims.Where(x => !existingClaims.Any(y => y.Contains(x))).ToList();
        }
    }
}