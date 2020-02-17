using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Models;
using Test.SimpleAuth.Shared.Mock.Repositories;
using Test.SimpleAuth.Shared.Mock.Services;
using PermissionGroup = SimpleAuth.Shared.Domains.PermissionGroup;
using Shared_ProjectRegistrableModules = SimpleAuth.Shared.ProjectRegistrableModules;
using Services_ProjectRegistrableModules = SimpleAuth.Services.ProjectRegistrableModules;
//using Sql_ProjectRegistrableModules = SimpleAuth.Sqlite.ProjectRegistrableModules;
using Sql_ProjectRegistrableModules = SimpleAuth.InMemoryDb.ProjectRegistrableModules;

namespace Test.Shared
{
    public abstract class BaseTestClass
    {
        protected virtual IServiceProvider Prepare()
        {
            return Prepare(x => { });
        }

        protected virtual IServiceProvider Prepare(Action<IServiceCollection> registerSvcAct)
        {
            var services = new ServiceCollection();
            RegisteredServices(services);
            registerSvcAct(services);
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

            serviceCollection.AddSingleton(MLog<DefaultEncryptionService>().Object);
        }

        protected Mock<T> M<T>(MockBehavior mockBehavior = MockBehavior.Strict) where T : class
        {
            return new Mock<T>(mockBehavior);
        }

        protected Mock<ILogger<T>> MLog<T>(MockBehavior mockBehavior = MockBehavior.Strict)
        {
            var logger = M<ILogger<T>>(mockBehavior);
            logger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>) It.IsAny<object>())
            );
            return logger;
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

        protected IEnumerable<PermissionGroup> YieldGroup(params string[] groupIdentities)
        {
            foreach (var tp in groupIdentities)
            {
                var spl = tp.Split('.', 3);

                Assert.AreEqual(3, spl.Length);
                yield return new PermissionGroup
                {
                    Name = spl[0],
                    Corp = spl[1],
                    App = spl[2]
                };
            }
        }

        protected string RandomText(int len = 5) => Guid.NewGuid().ToString().Replace("-", "").Substring(0, len);
        protected string RandomCorp() => RandomText(8);
        protected string RandomApp() => RandomText();
        protected string RandomEnv() => RandomText();
        protected string RandomTenant() => RandomText();
        protected string RandomModule() => RandomText();
        protected string RandomPermissionGroup() => RandomText();
        protected string RandomUser() => RandomText();
        protected string RandomEmail() => $"{RandomText()}@{RandomText(3)}.{RandomText(3)}";

        private static readonly Random Rad = new Random();
        protected bool RandomBool() => Rad.Next() % 2 == 0;

        protected Mock<TRepo> BasicSetup<TRepo, TEntity, TEntityKey>(Mock<TRepo> mockRepo)
            where TRepo : class, IRepository<TEntity, TEntityKey>
            where TEntity : BaseEntity<TEntityKey>
        {
            mockRepo.Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<TEntity>>())).ReturnsAsync(1);
            mockRepo.Setup(x => x.UpdateManyAsync(It.IsAny<IEnumerable<TEntity>>())).ReturnsAsync(1);
            mockRepo.Setup(x => x.DeleteManyAsync(It.IsAny<IEnumerable<TEntity>>())).ReturnsAsync(1);

            return mockRepo;
        }
    }
}