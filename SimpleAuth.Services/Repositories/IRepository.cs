using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<int> CreateManyAsync(IEnumerable<TEntity> entities);

        Task<TEntity> FindSingleAsync(IEnumerable<Expression<Func<TEntity, bool>>> expressions);

        IEnumerable<TEntity> FindMany(IEnumerable<Expression<Func<TEntity, bool>>> expressions,
            FindOptions findOptions = null);

        IEnumerable<TEntity> FindManyOrdered<TKey>(IEnumerable<Expression<Func<TEntity, bool>>> expressions,
            FindOptions findOptions = null,
            OrderByOptions<TEntity, TKey> orderByOption = null);

        Task<int> UpdateManyAsync(IEnumerable<TEntity> entities);
        Task<int> DeleteManyAsync(IEnumerable<TEntity> entities);

        Task TruncateTable();
    }

    public enum OrderDirection
    {
        Default = 0,
        Asc = 0,
        Desc = 1
    }

    public interface IRepository<TEntity, in TEntityKey> : IRepository<TEntity> where TEntity : BaseEntity<TEntityKey>
    {
        TEntity Find(TEntityKey id);
        Task<TEntity> FindAsync(TEntityKey id);
    }

    public abstract class Repository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly IDbContextFactory DbContextFactory;

        protected Repository(IDbContextFactory dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        internal DbContext OpenConnect()
        {
            return DbContextFactory.CreateDbContext();
        }

        public virtual async Task<int> CreateManyAsync(IEnumerable<TEntity> entities)
        {
            await using var ctx = OpenConnect();
            var dbSet = ctx.Set<TEntity>();
            foreach (var entity in entities)
                await dbSet.AddAsync(entity);
            return await ctx.SaveChangesAsync();
        }

        public async Task<TEntity> FindSingleAsync(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
        {
            await using var ctx = OpenConnect();
            var dbSet = Include(ctx.Set<TEntity>());

            return await expressions
                .Aggregate(
                    dbSet,
                    (current, expression) => current.Where(expression)
                )
                .SingleOrDefaultAsync();
        }

        public virtual IEnumerable<TEntity> FindMany(IEnumerable<Expression<Func<TEntity, bool>>> expressions,
            FindOptions findOptions = null)
        {
            using var ctx = OpenConnect();
            var dbSet = Include(ctx.Set<TEntity>());
            var queryable = FindManyQueryBuilder(dbSet, expressions);
            queryable = ApplyFindOption(queryable, findOptions);
            return queryable.ToImmutableArray();
        }

        public virtual IEnumerable<TEntity> FindManyOrdered<TKey>(
            IEnumerable<Expression<Func<TEntity, bool>>> expressions,
            FindOptions findOptions = null,
            OrderByOptions<TEntity, TKey> orderByOption = null)
        {
            using var ctx = OpenConnect();
            var dbSet = Include(ctx.Set<TEntity>());
            var queryable = FindManyQueryBuilder(dbSet, expressions);

            if (orderByOption?.Expression != null)
            {
                if (orderByOption.Direction == OrderDirection.Desc)
                    queryable = queryable.OrderByDescending(orderByOption.Expression);
                else
                    queryable = queryable.OrderBy(orderByOption.Expression);
            }

            queryable = ApplyFindOption(queryable, findOptions);

            return queryable.ToImmutableArray();
        }

        private IQueryable<TEntity> FindManyQueryBuilder(IQueryable<TEntity> queryable,
            IEnumerable<Expression<Func<TEntity, bool>>> expressions)
        {
            return expressions
                .Aggregate(
                    queryable,
                    (current, expression) => current.Where(expression)
                );
        }

        private IQueryable<TEntity> ApplyFindOption(IQueryable<TEntity> queryable,
            FindOptions findOptions = null)
        {
            findOptions ??= new FindOptions();

            if (findOptions.Skip > 0)
                queryable = queryable.Skip(findOptions.Skip);

            if (findOptions.Take > 0)
                queryable = queryable.Take(findOptions.Take);

            return queryable;
        }

        public virtual async Task<int> UpdateManyAsync(IEnumerable<TEntity> entities)
        {
            await using var ctx = OpenConnect();
            var dbSet = ctx.Set<TEntity>();

            foreach (var entity in entities)
                dbSet.Update(entity);

            return await ctx.SaveChangesAsync();
        }

        public virtual async Task<int> DeleteManyAsync(IEnumerable<TEntity> entities)
        {
            await using var ctx = OpenConnect();
            var dbSet = ctx.Set<TEntity>();
            foreach (var entity in entities)
                dbSet.Remove(entity);
            return await ctx.SaveChangesAsync();
        }

        public async Task TruncateTable()
        {
            await using var ctx = OpenConnect();
            var dbSet = ctx.Set<TEntity>();
            dbSet.RemoveRange(dbSet);
            await ctx.SaveChangesAsync();
        }

        protected virtual IQueryable<TEntity> Include(DbSet<TEntity> dbSet)
        {
            return dbSet;
        }
    }

    public abstract class Repository<TEntity, TEntityKey> :
        Repository<TEntity>,
        IRepository<TEntity, TEntityKey>
        where TEntity : BaseEntity<TEntityKey>
    {
        protected Repository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }

        public TEntity Find(TEntityKey id)
        {
            using var ctx = OpenConnect();
            var dbSet = Include(ctx.Set<TEntity>());
            return dbSet.SingleOrDefault(e => e.Id.Equals(id));
        }

        public async Task<TEntity> FindAsync(TEntityKey id)
        {
            await using var ctx = OpenConnect();
            var dbSet = Include(ctx.Set<TEntity>());
            return await dbSet.SingleOrDefaultAsync(e => e.Id.Equals(id));
        }
    }

    public class FindOptions
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }

    public class OrderByOptions<TSource, TKey>
    {
        public Expression<Func<TSource, TKey>> Expression { get; set; }
        public OrderDirection Direction { get; set; }
    }

    public static class RepositoryExtensions
    {
        public static async Task CreateAsync<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : BaseEntity
        {
            await repository.CreateManyAsync(new[] {entity});
        }

        public static IEnumerable<TEntity> FindOrdered<TEntity, TKey>(this IRepository<TEntity> repository,
            Expression<Func<TEntity, bool>> expression,
            FindOptions findOptions = null,
            OrderByOptions<TEntity, TKey> orderByOption = null) where TEntity : BaseEntity
        {
            return repository.FindManyOrdered(new[] {expression}, findOptions, orderByOption);
        }

        public static async Task<TEntity> FindSingleAsync<TEntity>(this IRepository<TEntity> repository,
            Expression<Func<TEntity, bool>> expression) where TEntity : BaseEntity
        {
            return await repository.FindSingleAsync(new[] {expression});
        }

        public static IEnumerable<TEntity> Find<TEntity>(this IRepository<TEntity> repository,
            Expression<Func<TEntity, bool>> expression,
            FindOptions findOptions = null) where TEntity : BaseEntity
        {
            return repository.FindMany(new[] {expression}, findOptions);
        }

        public static async Task UpdateAsync<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : BaseEntity
        {
            await repository.UpdateManyAsync(new[] {entity});
        }

        public static async Task DeleteAsync<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : BaseEntity
        {
            await repository.DeleteManyAsync(new[] {entity});
        }
    }
}