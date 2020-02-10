using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuth.Client.Models;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public interface IClaimTransformingService
    {
        Task<PackageSimpleAuthorizationClaim> TransformBackAsync(string data);
        Task<string> TransformAsync(PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim);
        Task<PackageSimpleAuthorizationClaim> UnpackAsync(Claim claim);
        Task<Claim> PackAsync(PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim);
    }

    public abstract class AbstractClaimTransformingService : IClaimTransformingService
    {
        public abstract Task<PackageSimpleAuthorizationClaim> TransformBackAsync(string data);

        public abstract Task<string> TransformAsync(PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim);


        public async Task<PackageSimpleAuthorizationClaim> UnpackAsync(Claim claim)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            if (claim.Type != SimpleAuthDefaults.ClaimType)
                throw new ArgumentException($"{nameof(claim)}: {nameof(claim.Type)}");

            return await TransformBackAsync(claim.Value);
        }

        public async Task<Claim> PackAsync(PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim)
        {
            var transformedData = await TransformAsync(packageSimpleAuthorizationClaim);
            return new Claim(SimpleAuthDefaults.ClaimType,
                transformedData,
                SimpleAuthDefaults.ClaimValueType,
                SimpleAuthDefaults.ClaimIssuer);
        }
    }

    public class SelfContainedClaimService : AbstractClaimTransformingService
    {
        public override async Task<PackageSimpleAuthorizationClaim> TransformBackAsync(string data)
        {
            await Task.CompletedTask;
            return JsonConvert.DeserializeObject<PackageSimpleAuthorizationClaim>(data);
        }

        public override async Task<string> TransformAsync(
            PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim)
        {
            await Task.CompletedTask;
            return JsonConvert.SerializeObject(packageSimpleAuthorizationClaim);
        }
    }

    public class LocalCachingClaimService : AbstractClaimTransformingService
    {
        private readonly ConcurrentDictionary<string, PackageSimpleAuthorizationClaim> _dict =
            new ConcurrentDictionary<string, PackageSimpleAuthorizationClaim>();

        public override async Task<PackageSimpleAuthorizationClaim> TransformBackAsync(string data)
        {
            await Task.CompletedTask;

            if (!_dict.TryGetValue(data, out var simpleAuthorizationClaims))
                return default;
            return simpleAuthorizationClaims;
        }

        public override Task<string> TransformAsync(PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim)
        {
            var generatedKey = Guid.NewGuid().ToString();

            _dict.AddOrUpdate(generatedKey,
                _ => packageSimpleAuthorizationClaim,
                (_, y) => packageSimpleAuthorizationClaim
            );

            return Task.FromResult(generatedKey);
        }
    }
}