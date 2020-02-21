using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Repositories
{
    public interface IPermissionGroupRepository : IRepository<PermissionGroup, Guid>
    {
        IEnumerable<PermissionGroup> Search(string term, string corp, string app, FindOptions findOptions = null);
        Task<int> UpdatePermissionRecordsAsync(PermissionGroup permissionGroup, List<PermissionRecord> newPermissions);
    }

    public class PermissionGroupRepository : Repository<PermissionGroup, Guid>, IPermissionGroupRepository
    {
        public PermissionGroupRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }

        protected override IQueryable<PermissionGroup> Include(DbSet<PermissionGroup> dbSet)
        {
            return base.Include(dbSet)
                .Include(x => x.PermissionRecords)
                .Include(x => x.PermissionGroupUsers);
        }

        public IEnumerable<PermissionGroup> Search(string term, string corp, string app, FindOptions findOptions = null)
        {
            if (corp == null)
                throw new ArgumentNullException(nameof(corp));
            
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            
            corp = corp.NormalizeInput();
            app = app.NormalizeInput();
            term = term?.Trim().NormalizeInput();

            findOptions ??= new FindOptions();

            var policies = new List<Expression<Func<PermissionGroup, bool>>>()
            {
                x => x.Corp == corp && x.App == app,
                x => !x.Locked,
            };

            if (!term.IsEmpty() && !"*".Equals(term))
                policies.Add(x => x.Name.Contains(term));

            return FindManyOrdered(policies, findOptions, new OrderByOptions<PermissionGroup,Guid>
            {
                Expression = x => x.Id,
                Direction = OrderDirection.Asc
            });
        }


        public async Task<int> UpdatePermissionRecordsAsync(PermissionGroup permissionGroup, List<PermissionRecord> newPermissions)
        {
            newPermissions = newPermissions.OrEmpty().Where(x => x.Verb != Verb.None).ToList();
            
            await using var ctx = OpenConnect();
            var dbGroups = ctx.Set<PermissionGroup>();
            var dbRecords = ctx.Set<PermissionRecord>();

            permissionGroup = await Include(dbGroups).SingleAsync(x => x.Id == permissionGroup.Id);
            
            if (permissionGroup.PermissionRecords.IsAny())
                dbRecords.RemoveRange(permissionGroup.PermissionRecords);

            if (newPermissions.IsAny())
            {
                newPermissions.ForEach(r =>
                {
                    r.WithRandomId();
                    ctx.Entry(r).State = EntityState.Added;
                });
                await dbRecords.AddRangeAsync(newPermissions);
            }

            permissionGroup.PermissionRecords = newPermissions;

            dbGroups.Update(permissionGroup);

            return await ctx.SaveChangesAsync();
        }

        public override async Task<int> DeleteManyAsync(IEnumerable<PermissionGroup> entities)
        {
            var ids = entities.Select(e => e.Id).Distinct().ToList();
            await using var ctx = OpenConnect();

            var gSet = ctx.Set<PermissionGroup>();
            var queryable = Include(gSet);

            var lookupEntities = await queryable.Where(x => ids.Any(id => x.Id == id)).ToListAsync();
            var missingIds = ids.Where(id => lookupEntities.All(le => le.Id != id)).ToList();
            if (missingIds.IsAny())
                throw new EntityNotExistsException(missingIds.Select(id => id.ToString()));

            var inUsedEntities = lookupEntities.Where(x => x.PermissionGroupUsers.IsAny()).ToList();
            if (inUsedEntities.Any())
                throw new SimpleAuthException(
                    $"Groups {string.Join(", ", inUsedEntities.Select(x => x.Name))} are in use");

            var rrSet = ctx.Set<PermissionRecord>();
            foreach (var g in lookupEntities)
            {
                rrSet.RemoveRange(g.PermissionRecords);
                g.PermissionRecords.Clear();
            }

            gSet.RemoveRange(lookupEntities);

            return await ctx.SaveChangesAsync();
        }
    }
}