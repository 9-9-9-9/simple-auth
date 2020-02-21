using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;

namespace Test.Shared.Extensions
{
    public static class MockRepoExtensions
    {
        public static Mock<TRepo> SetupFindSingleAsync<TRepo, TEntity, TKey>(this Mock<TRepo> mockRepo,
            TEntity result)
            where TEntity : BaseEntity<TKey>
            where TRepo : class, IRepository<TEntity, TKey>
        {
            mockRepo
                .Setup(x => x.FindSingleAsync(It.IsAny<IEnumerable<Expression<Func<TEntity, bool>>>>()))
                .Returns(Task.FromResult(result));
            return mockRepo;
        }
        
        public static Mock<TRepo> SetupCreateManyAsync<TRepo, TEntity, TKey>(this Mock<TRepo> mockRepo,
            int effectedRows)
            where TEntity : BaseEntity<TKey>
            where TRepo : class, IRepository<TEntity, TKey>
        {
            mockRepo
                .Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<TEntity>>()))
                .Returns(Task.FromResult(effectedRows));
            return mockRepo;
        }
        
        public static Mock<TRepo> SetupUpdateManyAsync<TRepo, TEntity, TKey>(this Mock<TRepo> mockRepo,
            int effectedRows)
            where TEntity : BaseEntity<TKey>
            where TRepo : class, IRepository<TEntity, TKey>
        {
            mockRepo
                .Setup(x => x.UpdateManyAsync(It.IsAny<IEnumerable<TEntity>>()))
                .Returns(Task.FromResult(effectedRows));
            return mockRepo;
        }
    }
}