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

            serviceCollection.AddTransient<IRoleGroupRepository, RoleGroupRepository>();
            serviceCollection.AddTransient<IRepository<RoleGroup>, RoleGroupRepository>();

            serviceCollection.AddTransient<IRoleRepository, RoleRepository>();
            serviceCollection.AddTransient<IRepository<Role>, RoleRepository>();

            serviceCollection.AddTransient<IUserRepository, UserRepository>();
            serviceCollection.AddTransient<IRepository<User>, UserRepository>();

            serviceCollection.AddTransient<IRoleRecordRepository, RoleRecordRepository>();
            serviceCollection.AddTransient<IRepository<RoleRecord>, RoleRecordRepository>();

            serviceCollection.AddTransient<ILocalUserInfoRepository, LocalUserInfoRepository>();
            serviceCollection.AddTransient<IRepository<LocalUserInfo>, LocalUserInfoRepository>();

            serviceCollection.AddTransient<IRoleGroupUserRepository, RoleGroupUserRepository>();
            serviceCollection.AddTransient<IRepository<RoleGroupUser>, RoleGroupUserRepository>();
            
            serviceCollection.AddTransient<ITokenInfoRepository, TokenInfoRepository>();
            serviceCollection.AddTransient<IRepository<TokenInfo>, TokenInfoRepository>();

            serviceCollection.AddSingleton<ICachedTokenInfoRepository, CachedTokenInfoRepository>();
        }
    }
}