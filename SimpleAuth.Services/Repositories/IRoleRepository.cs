using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared;

namespace SimpleAuth.Repositories
{
    public interface IRoleRepository : IRepository<Role, string>
    {
        IEnumerable<Role> Search(string term, string corp, string app, FindOptions findOptions = null);
    }

    public class RoleRepository : Repository<Role, string>, IRoleRepository
    {
        public RoleRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }

        public IEnumerable<Role> Search(string term, string corp, string app, FindOptions findOptions = null)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));

            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));

            term = term.NormalizeInput();
            corp = corp.NormalizeInput();
            app = app.NormalizeInput();

            findOptions ??= new FindOptions();

            var policies = new List<Expression<Func<Role, bool>>>
            {
                x => x.Corp == corp && x.App == app,
                x => !x.Locked
            };

            if (term != null && term != Constants.WildCard)
                policies.Add(x => x.Id.Contains(term));

            return FindManyOrdered(policies, findOptions, new OrderByOptions<Role, string>
            {
                Expression = x => x.Id,
                Direction = OrderDirection.Asc
            });
        }

#pragma warning disable 1998
        public override async Task<int> DeleteManyAsync(IEnumerable<Role> entities)
        {
            throw new NotSupportedException("Role is kind of static data thus no need to be deleted");
        }
#pragma warning restore 1998
    }
}