using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleAuth.Client.Models;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public interface IClaimCachingService
    {
        Task<IEnumerable<SimpleAuthorizationClaim>> GetClaimsAsync(string key);
        Task SaveClaimsAsync(string key, IEnumerable<SimpleAuthorizationClaim> claims);
    }

    /// <summary>
    /// *BEWARE* do not use this default implementation in multi-tenant architecture, should implement distributed cache
    /// </summary>
    public class ClaimLocalCachingService : IClaimCachingService
    {
        private readonly ConcurrentDictionary<string, IEnumerable<SimpleAuthorizationClaim>> _dict =
            new ConcurrentDictionary<string, IEnumerable<SimpleAuthorizationClaim>>();

        public Task<IEnumerable<SimpleAuthorizationClaim>> GetClaimsAsync(string key)
        {
            return Task.FromResult(
                _dict.TryGetValue(key, out var res) ? res : null
            );
        }

        public Task SaveClaimsAsync(string key, IEnumerable<SimpleAuthorizationClaim> claims)
        {
            _dict.AddOrUpdate(key, _ => claims, (_, y) => claims);
            return Task.CompletedTask;
        }
    }
}