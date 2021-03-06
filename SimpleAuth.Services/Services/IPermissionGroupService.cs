using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using PermissionGroup = SimpleAuth.Shared.Domains.PermissionGroup;

namespace SimpleAuth.Services
{
    public interface IPermissionGroupService : IDomainService
    {
        IEnumerable<PermissionGroup> SearchGroups(string term, string corp, string app, FindOptions findOptions = null);
        Task<PermissionGroup> GetGroupByNameAsync(string name, string corp, string app);
        IEnumerable<PermissionGroup> FindByName(string[] nameList, string corp, string app);
        Task AddGroupAsync(CreatePermissionGroupModel newPermissionGroup);
        Task UpdateLockStatusAsync(PermissionGroup permissionGroup);
        Task AddPermissionsToGroupAsync(PermissionGroup permissionGroup, PermissionModel[] permissionModels);
        Task DeletePermissionsFromGroupAsync(PermissionGroup permissionGroup, PermissionModel[] permissionModels);
        Task DeleteAllPermissionsFromGroupAsync(PermissionGroup permissionGroup);
        Task DeleteGroupAsync(PermissionGroup permissionGroup);
        Task UpdateGroupInfoAsync(PermissionGroup targetGroup, string corp, string app, PermissionGroup newInfo);
    }

    public class DefaultPermissionGroupService : DomainService<IPermissionGroupRepository, Entities.PermissionGroup>,
        IPermissionGroupService
    {
        private readonly IRoleRepository _roleRepository;

        public DefaultPermissionGroupService(IServiceProvider serviceProvider,
            IRoleRepository roleRepository) : base(serviceProvider)
        {
            _roleRepository = roleRepository;
        }

        public IEnumerable<PermissionGroup> SearchGroups(string term, string corp, string app,
            FindOptions findOptions = null)
        {
            return Repository.Search(term, corp, app, findOptions)
                .OrEmpty()
                .Select(x => x.ToDomainObject());
        }

        public async Task<PermissionGroup> GetGroupByNameAsync(string name, string corp, string app)
        {
            return (
                await Repository
                    .FindSingleAsync(
                        x =>
                            x.Name == name
                            &&
                            x.Corp == corp
                            &&
                            x.App == app
                    )
            )?.ToDomainObject();
        }

        public IEnumerable<PermissionGroup> FindByName(string[] nameList, string corp, string app)
        {
            var expressions = new List<Expression<Func<Entities.PermissionGroup, bool>>>
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
                .OrEmpty()
                .Select(x => x.ToDomainObject());
        }

        public async Task AddGroupAsync(CreatePermissionGroupModel newPermissionGroup)
        {
            var findSingleAsync = await Repository.FindSingleAsync(x =>
                x.Name == newPermissionGroup.Name
                && x.Corp == newPermissionGroup.Corp
                && x.App == newPermissionGroup.App
            );
            if (findSingleAsync != default)
                throw new EntityAlreadyExistsException(newPermissionGroup.Name);

            Permission[] initRoles = new Permission[0];
            if (newPermissionGroup.CopyFromPermissionGroups.IsAny())
            {
                var groupsToCopyFrom = FindByName(newPermissionGroup.CopyFromPermissionGroups, newPermissionGroup.Corp,
                        newPermissionGroup.App)
                    .ToArray();
                // ReSharper disable once AssignNullToNotNullAttribute
                var missingGroups = newPermissionGroup.CopyFromPermissionGroups
                    .Except(groupsToCopyFrom.Select(x => x.Name))
                    .ToArray();
                if (missingGroups.Any())
                    throw new EntityNotExistsException(missingGroups);

                initRoles = groupsToCopyFrom.SelectMany(x => x.Permissions).ToArray();
            }

            await Repository.CreateAsync(new Entities.PermissionGroup
            {
                Id = Guid.NewGuid(),
                Name = newPermissionGroup.Name,
                Description = newPermissionGroup.Description,
                Corp = newPermissionGroup.Corp,
                App = newPermissionGroup.App,
                Locked = false,
                PermissionRecords = initRoles
                    .Select(r => r.ToEntityObject().WithRandomId())
                    .ToList()
            });
        }

        public async Task UpdateLockStatusAsync(PermissionGroup permissionGroup)
        {
            if (permissionGroup == null)
                throw new ArgumentNullException(nameof(permissionGroup));

            var entity = await GetEntity(permissionGroup);
            if (entity.Locked == permissionGroup.Locked)
                return;

            entity.Locked = permissionGroup.Locked;
            await Repository.UpdateAsync(entity);
        }

        public async Task AddPermissionsToGroupAsync(PermissionGroup permissionGroup,
            PermissionModel[] permissionModels)
        {
            if (permissionGroup == null)
                throw new ArgumentNullException(nameof(permissionGroup));
            if (permissionModels == null)
                throw new ArgumentNullException(nameof(permissionModels));
            if (permissionModels.IsEmpty() || permissionModels.Any(x => x == null))
                throw new ArgumentException(nameof(permissionModels));
            if (permissionGroup.Permissions.IsAny())
                throw new InvalidOperationException(
                    $"Domain object {nameof(permissionGroup)} should not contains value in property {nameof(permissionGroup.Permissions)} to prevent un-expected operation");

            var acceptRolePrefix =
                string.Join(Constants.SplitterRoleParts, permissionGroup.Corp, permissionGroup.App, string.Empty);

            var crossAppRoleModels = permissionModels.Where(x => !x.Role.StartsWith(acceptRolePrefix)).ToArray();
            if (crossAppRoleModels.Any())
                throw new SimpleAuthSecurityException(string.Join(',', crossAppRoleModels.Select(x => x.Role)));

            var newRoles =
                permissionModels.Select(r => new Permission
                    {
                        RoleId = r.Role,
                        Verb = r.Verb.Deserialize()
                    })
                    .DistinctRoles()
                    .Select(r => r.ToEntityObject())
                    .ToList();

            await UpdateRolesAsync(permissionGroup, newRoles);
        }

        public async Task DeletePermissionsFromGroupAsync(PermissionGroup permissionGroup,
            PermissionModel[] permissionModels)
        {
            if (permissionGroup == null)
                throw new ArgumentNullException(nameof(permissionGroup));
            if (permissionModels == null)
                throw new ArgumentNullException(nameof(permissionModels));
            if (permissionModels.IsEmpty() || permissionModels.Any(x => x == null))
                throw new ArgumentException(nameof(permissionModels));
            if (permissionGroup.Permissions.IsAny())
                throw new InvalidOperationException(
                    $"Domain object {nameof(permissionGroup)} should not contains value in property {nameof(permissionGroup.Permissions)} to prevent un-expected operation");

            var acceptRolePrefix =
                string.Join(Constants.SplitterRoleParts, permissionGroup.Corp, permissionGroup.App, string.Empty);

            var crossAppRoleModels = permissionModels.Where(x => !x.Role.StartsWith(acceptRolePrefix)).ToArray();
            if (crossAppRoleModels.Any())
                throw new SimpleAuthSecurityException(string.Join(',', crossAppRoleModels.Select(x => x.Role)));

            var domainGroup =
                await GetGroupByNameAsync(permissionGroup.Name, permissionGroup.Corp, permissionGroup.App);

            if (domainGroup == default)
                throw new EntityNotExistsException($"{permissionGroup.Name}");

            if (domainGroup.Permissions.IsEmpty())
                return;

            foreach (var permissionModel in permissionModels)
            {
                var matchingRole = domainGroup.Permissions.FirstOrDefault(x => x.RoleId == permissionModel.Role);
                if (matchingRole == default)
                    continue;
                matchingRole.Verb = matchingRole.Verb.Revoke(permissionModel.Verb.Deserialize());
            }

            await UpdateRolesAsync(domainGroup, domainGroup.Permissions
                .Select(r => r.ToEntityObject().WithRandomId())
                .ToList()
            );
        }

        public async Task DeleteAllPermissionsFromGroupAsync(PermissionGroup permissionGroup)
        {
            if (permissionGroup == null)
                throw new ArgumentNullException(nameof(permissionGroup));

            var domain = await GetGroupByNameAsync(permissionGroup.Name, permissionGroup.Corp, permissionGroup.App);

            if (domain == default)
                throw new EntityNotExistsException($"{permissionGroup.Name}");

            if (domain.Permissions.IsEmpty())
                return;

            await UpdateRolesAsync(domain, null);
        }

        public async Task DeleteGroupAsync(PermissionGroup permissionGroup)
        {
            if (permissionGroup == null)
                throw new ArgumentNullException(nameof(permissionGroup));

            var g = await Repository.FindSingleAsync(x =>
                x.Name == permissionGroup.Name
                &&
                x.Corp == permissionGroup.Corp
                &&
                x.App == permissionGroup.App);

            if (g == null)
                throw new EntityNotExistsException(permissionGroup.Name);

            await Repository.DeleteAsync(g);
        }

        public async Task UpdateGroupInfoAsync(PermissionGroup targetGroup, string corp, string app,
            PermissionGroup newInfo)
        {
            var permissionGroup =
                await Repository.FindSingleAsync(x => x.Name == targetGroup.Name && x.Corp == corp && x.App == app);

            if (permissionGroup == null)
                throw new EntityNotExistsException(targetGroup.Name);

            if (newInfo.Name != null)
                permissionGroup.Name = newInfo.Name;
            if (newInfo.Description != null)
                permissionGroup.Description = newInfo.Description;

            await Repository.UpdateAsync(permissionGroup);
        }

        private async Task UpdateRolesAsync(PermissionGroup permissionGroup, List<PermissionRecord> newRoles)
        {
            newRoles = newRoles.OrEmpty().ToList();

            foreach (var newRole in newRoles)
            {
                var role = await _roleRepository.FindSingleAsync(r => r.Id == newRole.RoleId);
                if (role == default)
                    throw new EntityNotExistsException(newRole.RoleId);
                newRole.Env = role.Env;
                newRole.Tenant = role.Tenant;
            }

            await Repository.UpdatePermissionRecordsAsync(await GetEntity(permissionGroup), newRoles);

            permissionGroup.Permissions = newRoles.Select(r => r.ToDomainObject()).ToArray();
        }

        private async Task<Entities.PermissionGroup> GetEntity(PermissionGroup permissionGroup)
        {
            var entity = await Repository.FindSingleAsync(x =>
                x.Name == permissionGroup.Name
                && x.Corp == permissionGroup.Corp
                && x.App == permissionGroup.App
            );
            if (entity == null)
                throw new EntityNotExistsException(permissionGroup.Name);
            return entity;
        }
    }
}