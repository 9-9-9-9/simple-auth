using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using Role = SimpleAuth.Shared.Domains.Role;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Services
{
    public interface IRoleService : IDomainService
    {
        Task AddRoleAsync(CreateRoleModel model);
        Task UpdateLockStatus(Role role);
        IEnumerable<Role> SearchRoles(string term, string corp, string app, FindOptions findOptions = null);
    }

    public class DefaultRoleService : DomainService<IRoleRepository, Entities.Role>, IRoleService
    {
        public DefaultRoleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task AddRoleAsync(CreateRoleModel model)
        {
            var entity = model.ConvertToEntity();

            if (Repository.Find(entity.Id) != null)
                throw new EntityAlreadyExistsException(entity.Id);
            
            await Repository.CreateAsync(entity);
        }

        public async Task UpdateLockStatus(Role role)
        {
            var entity = Repository.Find(role.RoleId);
            if (entity == null)
                throw new EntityNotExistsException(role.RoleId);

            entity.Locked = role.Locked;
            
            await Repository.UpdateAsync(entity);
        }

        public IEnumerable<Role> SearchRoles(string term, string corp, string app, FindOptions findOptions = null)
        {
            return Repository.Search(term, corp, app, findOptions).Select(x => new Role
            {
                RoleId = x.Id
            });
        }
    }
}