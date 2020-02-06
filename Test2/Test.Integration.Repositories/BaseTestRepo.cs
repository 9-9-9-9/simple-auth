using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using Test.Shared;

namespace Test.Integration.Repositories
{
    public class BaseTestRepo : BaseTestClass
    {
        protected async Task<(IRoleRepository, string)> GenerateRolesAsync()
        {
            var roleRepository = Svc<IRoleRepository>();
            var corp = RandomCorp();

            await roleRepository.CreateManyAsync(new List<Role>
            {
                new Role
                {
                    Corp = corp,
                    App = "a1",
                    Env = "e",
                    Tenant = "t",
                    Module = "m1"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a1",
                    Env = "e",
                    Tenant = "t",
                    Module = "m2"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a2",
                    Env = "e",
                    Tenant = "t",
                    Module = "m1"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a2",
                    Env = "e",
                    Tenant = "t",
                    Module = "m2"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a2",
                    Env = "e",
                    Tenant = "t",
                    Module = "m3"
                }.ComputeId(),
                new Role
                {
                    Corp = corp,
                    App = "a2",
                    Env = "e",
                    Tenant = "t",
                    Module = "m4"
                }.ComputeId(),
            });

            return (roleRepository, corp);
        }

        protected async Task<(IRoleGroupRepository, string)> GenerateGroupsAsync()
        {
            var roleGroupRepository = Svc<IRoleGroupRepository>();
            var corp = RandomCorp();

            await roleGroupRepository.CreateManyAsync(new[]
            {
                new RoleGroup
                {
                    Corp = corp,
                    App = "a1",
                    Name = "g11",
                    Locked = false,
                }.WithRandomId(),
                new RoleGroup
                {
                    Corp = corp,
                    App = "a1",
                    Name = "g12",
                    Locked = false,
                }.WithRandomId(),
                new RoleGroup
                {
                    Corp = corp,
                    App = "a2",
                    Name = "g21",
                    Locked = true,
                }.WithRandomId(),
            });

            return (roleGroupRepository, corp);
        }

        protected async Task<(ITokenInfoRepository, string)> GenerateTokensAsync()
        {
            var tokenInfoRepository = Svc<ITokenInfoRepository>();
            var corp = RandomCorp();

            await tokenInfoRepository.CreateManyAsync(new[]
            {
                new TokenInfo
                {
                    Corp = corp,
                    App = "a1",
                    Version = 1
                }.WithRandomId(),
                new TokenInfo
                {
                    Corp = corp,
                    App = "a2",
                    Version = 2
                }.WithRandomId(),
                new TokenInfo
                {
                    Corp = corp,
                    App = "a3",
                    Version = 3
                }.WithRandomId(),
            });

            return (tokenInfoRepository, corp);
        }
    }
}