using System.Collections.Generic;
using SimpleAuth.Shared.Domains;

namespace SimpleAuth.Repositories
{
    public interface ICachedUserRolesRepository : IMemoryCachedRepository<IEnumerable<Role>>
    {
    }

    public class CachedUserRolesRepository : MemoryCachedRepository<IEnumerable<Role>>, ICachedUserRolesRepository
    {
    }
}