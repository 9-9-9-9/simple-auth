using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using Test.Shared;
using Test.Shared.Utils;

namespace Test.SimpleAuth.Services.Test.Services
{
    public abstract class BaseTestService<TRepo, TEntity, TEntityKey> : BaseTestClass
    where TRepo : class, IRepository<TEntity>
    where TEntity : BaseEntity<TEntityKey>
    {
        protected IServiceProvider Prepare(out Mock<TRepo> mockRepo)
        {
            mockRepo = Mu.Of<TRepo>();
            var repoObj = mockRepo.Object;
            return Prepare(services => { services.AddSingleton(repoObj); });
        }
        
        protected IServiceProvider Prepare<TRepo2, TEntity2, TEntityKey2>(out Mock<TRepo> mockRepo, out Mock<TRepo2> mockRepo2)
            where TRepo2 : class, IRepository<TEntity2>
            where TEntity2 : BaseEntity<TEntityKey2>
        {
            mockRepo = Mu.Of<TRepo>();
            mockRepo2 = Mu.Of<TRepo2>();
            var repoObj = mockRepo.Object;
            var repoObj2 = mockRepo2.Object;
            return Prepare(services =>
            {
                services.AddSingleton(repoObj);
                services.AddSingleton(repoObj2);
            });
        }
        
        protected IServiceProvider Prepare<TRepo2, TEntity2>(out Mock<TRepo> mockRepo, out Mock<TRepo2> mockRepo2)
            where TRepo2 : class, IRepository<TEntity2>
            where TEntity2 : BaseEntity
        {
            mockRepo = Mu.Of<TRepo>();
            mockRepo2 = Mu.Of<TRepo2>();
            var repoObj = mockRepo.Object;
            var repoObj2 = mockRepo2.Object;
            return Prepare(services =>
            {
                services.AddSingleton(repoObj);
                services.AddSingleton(repoObj2);
            });
        }
    }
}