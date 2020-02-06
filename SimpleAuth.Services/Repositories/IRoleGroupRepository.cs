using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Repositories
{
    public interface IRoleGroupRepository : IRepository<RoleGroup, Guid>
    {
        IEnumerable<RoleGroup> Search(string term, string corp, string app, FindOptions findOptions = null);
        Task<int> UpdateRoleRecordsAsync(RoleGroup roleGroup, List<RoleRecord> newRoles);
    }

    public class RoleGroupRepository : Repository<RoleGroup, Guid>, IRoleGroupRepository
    {
        public RoleGroupRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }

        protected override IQueryable<RoleGroup> Include(DbSet<RoleGroup> dbSet)
        {
            return base.Include(dbSet)
                .Include(x => x.RoleRecords)
                .Include(x => x.RoleGroupUsers);
        }

        public IEnumerable<RoleGroup> Search(string term, string corp, string app, FindOptions findOptions = null)
        {
            if (corp == null)
                throw new ArgumentNullException(nameof(corp));
            
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            
            corp = corp.NormalizeInput();
            app = app.NormalizeInput();
            term = term?.Trim().NormalizeInput();

            findOptions ??= new FindOptions();

            var policies = new List<Expression<Func<RoleGroup, bool>>>()
            {
                x => x.Corp == corp && x.App == app,
                x => !x.Locked,
            };

            if (!term.IsEmpty() && !"*".Equals(term))
                policies.Add(x => x.Name.Contains(term));

            return FindManyOrdered(policies, findOptions, new OrderByOptions<RoleGroup,Guid>
            {
                Expression = x => x.Id,
                Direction = OrderDirection.Asc
            });
        }


        public async Task<int> UpdateRoleRecordsAsync(RoleGroup roleGroup, List<RoleRecord> newRoles)
        {
            newRoles = newRoles.OrEmpty().Where(x => x.Permission != Permission.None).ToList();
            
            await using var ctx = OpenConnect();
            var dbGroups = ctx.Set<RoleGroup>();
            var dbRecords = ctx.Set<RoleRecord>();

            roleGroup = await Include(dbGroups).SingleAsync(x => x.Id == roleGroup.Id);
            
            if (roleGroup.RoleRecords.IsAny())
                dbRecords.RemoveRange(roleGroup.RoleRecords);

            if (newRoles.IsAny())
            {
                newRoles.ForEach(r =>
                {
                    r.WithRandomId();
                    ctx.Entry(r).State = EntityState.Added;
                });
                await dbRecords.AddRangeAsync(newRoles);
            }

            roleGroup.RoleRecords = newRoles;

            dbGroups.Update(roleGroup);

            return await ctx.SaveChangesAsync();
        }

        public override async Task<int> DeleteManyAsync(IEnumerable<RoleGroup> entities)
        {
            var ids = entities.Select(e => e.Id).Distinct().ToList();
            await using var ctx = OpenConnect();

            var gSet = ctx.Set<RoleGroup>();
            var queryable = Include(gSet);

            var lookupEntities = await queryable.Where(x => ids.Any(id => x.Id == id)).ToListAsync();
            var missingIds = ids.Where(id => lookupEntities.All(le => le.Id != id)).ToList();
            if (missingIds.IsAny())
                throw new EntityNotExistsException(missingIds.Select(id => id.ToString()));

            var inUsedEntities = lookupEntities.Where(x => x.RoleGroupUsers.IsAny()).ToList();
            if (inUsedEntities.Any())
                throw new SimpleAuthException(
                    $"Groups {string.Join(", ", inUsedEntities.Select(x => x.Name))} are in use");

            var rrSet = ctx.Set<RoleRecord>();
            foreach (var g in lookupEntities)
            {
                rrSet.RemoveRange(g.RoleRecords);
                g.RoleRecords.Clear();
            }

            gSet.RemoveRange(lookupEntities);

            return await ctx.SaveChangesAsync();
        }
    }
}