using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public class RepositoryModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDbContextFactory, DefaultDbContextFactory>();

            serviceCollection.AddTransient<IPermissionGroupRepository, PermissionGroupRepository>();
            serviceCollection.AddTransient<IRepository<PermissionGroup>, PermissionGroupRepository>();

            serviceCollection.AddTransient<IRoleRepository, RoleRepository>();
            serviceCollection.AddTransient<IRepository<Role>, RoleRepository>();

            serviceCollection.AddTransient<IUserRepository, UserRepository>();
            serviceCollection.AddTransient<IRepository<User>, UserRepository>();

            serviceCollection.AddTransient<IPermissionRecordRepository, PermissionRecordRepository>();
            serviceCollection.AddTransient<IRepository<PermissionRecord>, PermissionRecordRepository>();

            serviceCollection.AddTransient<ILocalUserInfoRepository, LocalUserInfoRepository>();
            serviceCollection.AddTransient<IRepository<LocalUserInfo>, LocalUserInfoRepository>();

            serviceCollection.AddTransient<IPermissionGroupUserRepository, PermissionGroupUserRepository>();
            serviceCollection.AddTransient<IRepository<PermissionGroupUser>, PermissionGroupUserRepository>();
            
            serviceCollection.AddTransient<ITokenInfoRepository, TokenInfoRepository>();
            serviceCollection.AddTransient<IRepository<TokenInfo>, TokenInfoRepository>();
            
            serviceCollection.AddTransient<ICachedRepository<TokenInfo>, MemoryCachedRepository<TokenInfo>>();
            serviceCollection.AddTransient<IMemoryCachedRepository<TokenInfo>, MemoryCachedRepository<TokenInfo>>();

            serviceCollection.AddSingleton<ICachedTokenInfoRepository, CachedTokenInfoRepository>();
        }
    }
}