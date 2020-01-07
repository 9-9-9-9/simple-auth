using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using Role = SimpleAuth.Shared.Domains.Role;
using RoleGroup = SimpleAuth.Shared.Domains.RoleGroup;

namespace SimpleAuth.Services
{
    public interface IRoleGroupService : IDomainService
    {
        IEnumerable<RoleGroup> SearchRoleGroups(string term, string corp, string app, FindOptions findOptions = null);
        RoleGroup GetRoleGroupByName(string name, string corp, string app);
        IEnumerable<RoleGroup> FindByName(string[] nameList, string corp, string app);
        Task AddRoleGroupAsync(CreateRoleGroupModel newRoleGroup);
        Task UpdateLockStatusAsync(RoleGroup roleGroup);
        Task AddRolesToGroupAsync(RoleGroup roleGroup, RoleModel[] roleModels);
        Task DeleteRolesFromGroupAsync(RoleGroup roleGroup, RoleModel[] roleModels);
        Task DeleteAllRolesFromGroupAsync(RoleGroup roleGroup);
        Task DeleteRoleGroupAsync(RoleGroup roleGroup);
    }

    public class DefaultRoleGroupService : DomainService<IRoleGroupRepository, Entities.RoleGroup>,
        IRoleGroupService
    {
        private readonly ICachedUserRolesRepository _cachedUserRolesRepository;

        public DefaultRoleGroupService(IServiceProvider serviceProvider,
            ICachedUserRolesRepository cachedUserRolesRepository) : base(serviceProvider)
        {
            _cachedUserRolesRepository = cachedUserRolesRepository;
        }

        public IEnumerable<RoleGroup> SearchRoleGroups(string term, string corp, string app,
            FindOptions findOptions = null)
        {
            return Repository.Search(term, corp, app, findOptions)
                .Select(x => x.ToDomainObject());
        }

        public RoleGroup GetRoleGroupByName(string name, string corp, string app)
        {
            return GetRoleGroupByExpression(x => x.Name == name, corp, app, new FindOptions {Take = 1})
                .FirstOrDefault();
        }

        private IEnumerable<RoleGroup> GetRoleGroupByExpression(
            Expression<Func<Entities.RoleGroup, bool>> expression,
            string corp, string app, FindOptions findOptions = null)
        {
            return Repository
                .FindMany(new[]
                {
                    expression,
                    x =>
                        x.Corp == corp
                        &&
                        x.App == app
                }, findOptions)
                .Select(x => x.ToDomainObject());
        }

        public IEnumerable<RoleGroup> FindByName(string[] nameList, string corp, string app)
        {
            var expressions = new List<Expression<Func<Entities.RoleGroup, bool>>>
            {
                x =>
                    x.Corp == corp
                    &&
                    x.App == app
            };

            if (nameList.IsAny())
            {
                expressions.Add(x => nameList.Contains(x.Name));
            }

            return Repository
                .FindMany(expressions)
                .Select(x => x.ToDomainObject());
        }

        public async Task AddRoleGroupAsync(CreateRoleGroupModel newRoleGroup)
        {
            if (Repository.Find(x => x.Name == newRoleGroup.Name
                                     && x.Corp == newRoleGroup.Corp
                                     && x.App == newRoleGroup.App, new FindOptions {Take = 1}).Any())
            {
                throw new EntityAlreadyExistsException(newRoleGroup.Name);
            }

            Role[] initRoles = new Role[0];
            if (newRoleGroup.CopyFromRoleGroups.IsAny())
            {
                var groupsToCopyFrom = FindByName(newRoleGroup.CopyFromRoleGroups, newRoleGroup.Corp, newRoleGroup.App)
                    .ToArray();
                // ReSharper disable once AssignNullToNotNullAttribute
                var missingGroups = newRoleGroup.CopyFromRoleGroups.Except(groupsToCopyFrom.Select(x => x.Name))
                    .ToArray();
                if (missingGroups.Any())
                    throw new EntityNotExistsException(missingGroups);

                initRoles = groupsToCopyFrom.SelectMany(x => x.Roles).ToArray();
            }

            await Repository.CreateAsync(new Entities.RoleGroup
            {
                Id = Guid.NewGuid(),
                Name = newRoleGroup.Name,
                Corp = newRoleGroup.Corp,
                App = newRoleGroup.App,
                Locked = false,
                RoleRecords = initRoles
                    .Select(r => r.ToEntityObject().WithRandomId())
                    .ToList()
            });
        }

        public async Task UpdateLockStatusAsync(RoleGroup roleGroup)
        {
            var entity = await GetEntity(roleGroup);
            entity.Locked = roleGroup.Locked;

            _cachedUserRolesRepository.Clear(entity.Corp, entity.App);

            await Repository.UpdateAsync(entity);
        }

        public async Task AddRolesToGroupAsync(RoleGroup roleGroup, RoleModel[] roleModels)
        {
            var acceptRolePrefix =
                string.Join(Constants.SplitterRoleParts, roleGroup.Corp, roleGroup.App, string.Empty);

            var crossAppRoleModels = roleModels.Where(x => !x.Role.StartsWith(acceptRolePrefix)).ToArray();
            if (crossAppRoleModels.IsAny())
                throw new SimpleAuthSecurityException(string.Join(',', crossAppRoleModels.Select(x => x.Role)));

            var newRoles = roleGroup.Roles
                .OrEmpty()
                .Concat(
                    roleModels.Select(r => new Role
                    {
                        RoleId = r.Role,
                        Permission = r.Permission.Deserialize()
                    })
                )
                .DistinctRoles()
                .Select(r => r.ToEntityObject().WithRandomId())
                .ToList();

            await UpdateRolesAsync(roleGroup, newRoles);
        }

        public async Task DeleteRolesFromGroupAsync(RoleGroup roleGroup, RoleModel[] roleModels)
        {
            if (roleGroup.Roles.IsEmpty())
                return;

            foreach (var roleModel in roleModels)
            {
                var matchingRole = roleGroup.Roles.FirstOrDefault(x => x.RoleId == roleModel.Role);
                if (matchingRole == default)
                    continue;
                matchingRole.Permission = matchingRole.Permission.Revoke(roleModel.Permission.Deserialize());
            }

            await UpdateRolesAsync(roleGroup, roleGroup.Roles
                .Select(r => r.ToEntityObject().WithRandomId())
                .ToList()
            );
        }

        public async Task DeleteAllRolesFromGroupAsync(RoleGroup roleGroup)
        {
            if (roleGroup.Roles.IsEmpty())
                return;

            await UpdateRolesAsync(roleGroup, null);
        }

        public async Task DeleteRoleGroupAsync(RoleGroup roleGroup)
        {
            var g = Repository.Find(x =>
                x.Name == roleGroup.Name
                &&
                x.Corp == roleGroup.Corp
                &&
                x.App == roleGroup.App).FirstOrDefault();

            if (g == null)
                throw new EntityNotExistsException(roleGroup.Name);

            await Repository.DeleteAsync(g);
        }

        private async Task UpdateRolesAsync(RoleGroup roleGroup, List<RoleRecord> newRoles)
        {
            newRoles = newRoles.OrEmpty().Where(r => r.Permission != Permission.None).ToList();

            _cachedUserRolesRepository.Clear(roleGroup.Corp, roleGroup.App);

            await Repository.UpdateRoleRecordsAsync(await GetEntity(roleGroup), newRoles);

            roleGroup.Roles = newRoles.Select(r => r.ToDomainObject()).ToArray();
        }

        private async Task<Entities.RoleGroup> GetEntity(RoleGroup roleGroup)
        {
            var entity = await Repository.FindSingleAsync(x =>
                x.Name == roleGroup.Name
                && x.Corp == roleGroup.Corp
                && x.App == roleGroup.App
            );
            if (entity == null)
                throw new EntityNotExistsException(roleGroup.Name);
            return entity;
        }
    }
}