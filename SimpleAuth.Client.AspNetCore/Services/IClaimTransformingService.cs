using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuth.Client.Models;
using SimpleAuth.Shared;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public interface IClaimTransformingService
    {
        Task<ICollection<SimpleAuthorizationClaim>> TransformBackAsync(string data);
        Task<string> TransformAsync(ICollection<SimpleAuthorizationClaim> simpleAuthorizationClaims);
        Task<ICollection<SimpleAuthorizationClaim>> UnpackAsync(Claim claim);
        Task<Claim> PackAsync(ICollection<SimpleAuthorizationClaim> simpleAuthorizationClaims);
    }

    public abstract class AbstractClaimTransformingService : IClaimTransformingService
    {
        public abstract Task<ICollection<SimpleAuthorizationClaim>> TransformBackAsync(string data);

        public abstract Task<string> TransformAsync(ICollection<SimpleAuthorizationClaim> simpleAuthorizationClaims);


        public async Task<ICollection<SimpleAuthorizationClaim>> UnpackAsync(Claim claim)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            if (claim.Type != SimpleAuthDefaults.ClaimType)
                throw new ArgumentException(nameof(claim));

            if (claim.ValueType != nameof(SimpleAuthorizationClaim))
                throw new ArgumentException(nameof(claim));

            if (claim.Issuer != Constants.Identity.Issuer)
                throw new ArgumentException(nameof(claim));

            return await TransformBackAsync(claim.Value);
        }

        public async Task<Claim> PackAsync(ICollection<SimpleAuthorizationClaim> simpleAuthorizationClaims)
        {
            var transformedData = await TransformAsync(simpleAuthorizationClaims);
            return new Claim(SimpleAuthDefaults.ClaimType,
                transformedData,
                nameof(SimpleAuthorizationClaim),
                Constants.Identity.Issuer);
        }
    }

    public class SelfContainedClaimService : AbstractClaimTransformingService
    {
        public override async Task<ICollection<SimpleAuthorizationClaim>> TransformBackAsync(string data)
        {
            await Task.CompletedTask;
            return JsonConvert.DeserializeObject<ICollection<SimpleAuthorizationClaim>>(data);
        }

        public override async Task<string> TransformAsync(
            ICollection<SimpleAuthorizationClaim> simpleAuthorizationClaims)
        {
            await Task.CompletedTask;
            return JsonConvert.SerializeObject(simpleAuthorizationClaims);
        }
    }

    public class LocalCachingClaimService : AbstractClaimTransformingService
    {
        private readonly ConcurrentDictionary<string, ICollection<SimpleAuthorizationClaim>> _dict =
            new ConcurrentDictionary<string, ICollection<SimpleAuthorizationClaim>>();

        public override async Task<ICollection<SimpleAuthorizationClaim>> TransformBackAsync(string data)
        {
            await Task.CompletedTask;

            if (_dict.TryGetValue(data, out var simpleAuthorizationClaims))
                return simpleAuthorizationClaims;

            return Enumerable.Empty<SimpleAuthorizationClaim>().ToList();
        }

        public override Task<string> TransformAsync(ICollection<SimpleAuthorizationClaim> simpleAuthorizationClaims)
        {
            var generatedKey = Guid.NewGuid().ToString();

            _dict.AddOrUpdate(generatedKey,
                _ => simpleAuthorizationClaims,
                (_, y) => simpleAuthorizationClaims);

            return Task.FromResult(generatedKey);
        }
    }
}