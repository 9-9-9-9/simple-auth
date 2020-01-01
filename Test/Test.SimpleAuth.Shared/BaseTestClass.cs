using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Models;
using Test.SimpleAuth.Shared.Mock.Repositories;
using Test.SimpleAuth.Shared.Mock.Services;
using Shared_ProjectRegistrableModules = SimpleAuth.Shared.ProjectRegistrableModules;
using Services_ProjectRegistrableModules = SimpleAuth.Services.ProjectRegistrableModules;
//using Sql_ProjectRegistrableModules = SimpleAuth.Sqlite.ProjectRegistrableModules;
using Sql_ProjectRegistrableModules = SimpleAuth.InMemoryDb.ProjectRegistrableModules;
using RoleGroup = SimpleAuth.Shared.Domains.RoleGroup;
using User = SimpleAuth.Shared.Domains.User;

namespace Test.SimpleAuth.Shared
{
    public abstract class BaseTestClass
    {
        protected virtual IServiceProvider Prepare()
        {
            var services = new ServiceCollection();
            RegisteredServices(services);
            return services.BuildServiceProvider();
        }

        private readonly HashSet<(Type, Type, string)> _truncatedByName = new HashSet<(Type, Type, string)>();

        protected virtual async Task Truncate<TEntity>(string name) where TEntity : BaseEntity
        {
            var tKey = (this.GetType(), typeof(TEntity), name);
            if (_truncatedByName.Contains(tKey))
                return;
            _truncatedByName.Add(tKey);
            var repo = Svc<IRepository<TEntity>>();
            await repo.TruncateTable();
        }

        protected virtual async Task Truncate<TEntity1, TEntity2>(string name)
            where TEntity1 : BaseEntity
            where TEntity2 : BaseEntity
        {
            await Truncate<TEntity1>(name);
            await Truncate<TEntity2>(name);
        }

        protected virtual async Task Truncate<TEntity1, TEntity2, TEntity3>(string name)
            where TEntity1 : BaseEntity
            where TEntity2 : BaseEntity
            where TEntity3 : BaseEntity
        {
            await Truncate<TEntity1>(name);
            await Truncate<TEntity2>(name);
            await Truncate<TEntity3>(name);
        }

        protected virtual async Task Truncate<TEntity1, TEntity2, TEntity3, TEntity4>(string name)
            where TEntity1 : BaseEntity
            where TEntity2 : BaseEntity
            where TEntity3 : BaseEntity
            where TEntity4 : BaseEntity
        {
            await Truncate<TEntity1>(name);
            await Truncate<TEntity2>(name);
            await Truncate<TEntity3>(name);
            await Truncate<TEntity4>(name);
        }

        protected virtual async Task Truncate<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>(string name)
            where TEntity1 : BaseEntity
            where TEntity2 : BaseEntity
            where TEntity3 : BaseEntity
            where TEntity4 : BaseEntity
            where TEntity5 : BaseEntity
        {
            await Truncate<TEntity1>(name);
            await Truncate<TEntity2>(name);
            await Truncate<TEntity3>(name);
            await Truncate<TEntity4>(name);
            await Truncate<TEntity5>(name);
        }

        protected virtual async Task Truncate<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(string name)
            where TEntity1 : BaseEntity
            where TEntity2 : BaseEntity
            where TEntity3 : BaseEntity
            where TEntity4 : BaseEntity
            where TEntity5 : BaseEntity
            where TEntity6 : BaseEntity
        {
            await Truncate<TEntity1>(name);
            await Truncate<TEntity2>(name);
            await Truncate<TEntity3>(name);
            await Truncate<TEntity4>(name);
            await Truncate<TEntity5>(name);
            await Truncate<TEntity6>(name);
        }

        private readonly HashSet<(Type, string)> _singleExecution = new HashSet<(Type, string)>();

        protected virtual void ExecuteOne(string name, Action act)
        {
            var tKey = (GetType(), name);
            if (_singleExecution.Contains(tKey))
                return;
            _singleExecution.Add(tKey);
            act();
        }

        protected virtual void RegisteredServices(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterModules<Shared_ProjectRegistrableModules>();
            serviceCollection.RegisterModules<Services_ProjectRegistrableModules>();
            serviceCollection.RegisterModules<Sql_ProjectRegistrableModules>();
            serviceCollection.AddTransient<SimpleAuthDbContext, DbContext>();
            serviceCollection.RegisterModules<MockServiceModules>();
            serviceCollection.RegisterModules<MockRepositoryModules>();
        }

        protected TService Svc<TService>()
        {
            return Prepare().GetRequiredService<TService>();
        }

        protected async Task AddRoleAsync(IRoleService svc, string corp, string app, string env, string tenant,
            string module,
            params string[] subModules)
        {
            await svc.AddRoleAsync(new CreateRoleModel
            {
                Corp = corp,
                App = app,
                Env = env,
                Tenant = tenant,
                Module = module,
                SubModules = subModules
            });
        }

        protected async Task AddRoleGroupAsync(IRoleGroupService svc, string name, string corp, string app,
            params string[] copyFrom)
        {
            await svc.AddRoleGroupAsync(new CreateRoleGroupModel
            {
                Name = name,
                Corp = corp,
                App = app,
                CopyFromRoleGroups = copyFrom
            });
        }

        protected IEnumerable<Role> YieldRoles(int from, int to)
        {
            for (var i = from; i <= to; i++)
            {
                var role = new Role
                {
                    Corp = "c",
                    App = "a",
                    Env = "e",
                    Tenant = "t",
                    Module = $"m{i}",
                };
                role.ComputeId();
                yield return role;
            }
        }

        protected async Task AssignUserToGroup(IUserService svc, string userId, string name, string corp, string app)
        {
            await svc.AssignUserToGroups(new User
                {
                    Id = userId,
                },
                new[]
                {
                    new RoleGroup
                    {
                        Name = name,
                        Corp = corp,
                        App = app
                    }
                }.ToArray());
        }

        protected async Task AssignUserToGroups(IUserService svc, string userId, params string[] groupIdentities)
        {
            await svc.AssignUserToGroups(new User
                {
                    Id = userId,
                },
                YieldGroup(groupIdentities)
                    .ToArray()
            );
        }

        protected IEnumerable<RoleGroup> YieldGroup(params string[] groupIdentities)
        {
            foreach (var tp in groupIdentities)
            {
                var spl = tp.Split('.', 3);

                Assert.AreEqual(3, spl.Length);
                yield return new RoleGroup
                {
                    Name = spl[0],
                    Corp = spl[1],
                    App = spl[2]
                };
            }
        }

        protected string RandomCorp() => Guid.NewGuid().ToString().Substring(0, 5);
        protected string RandomUser() => Guid.NewGuid().ToString().Substring(0, 5);
    }
}