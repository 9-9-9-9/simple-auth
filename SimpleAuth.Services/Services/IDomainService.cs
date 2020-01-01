using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Services
{
    public interface IDomainService
    {
    }

    public abstract class DomainService<TRepo, TEntity> : IDomainService
        where TRepo : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected TRepo Repository;

        protected DomainService(IServiceProvider serviceProvider)
        {
            Repository = serviceProvider.GetRequiredService<TRepo>();
        }
    }
}