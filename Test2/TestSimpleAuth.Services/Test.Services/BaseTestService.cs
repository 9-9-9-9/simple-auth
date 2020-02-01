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
    }
}