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
using SimpleAuth.Shared.Utils;
using LocalUserInfo = SimpleAuth.Shared.Domains.LocalUserInfo;
using PermissionGroup = SimpleAuth.Shared.Domains.PermissionGroup;
using User = SimpleAuth.Shared.Domains.User;

namespace SimpleAuth.Services
{
    public interface IUserService : IDomainService
    {
        User GetUser(string userId, string corp);
        Task CreateUserAsync(User user, LocalUserInfo localUserInfo);
        Task AssignUserToGroupsAsync(User user, PermissionGroup[] permissionGroups);
        Task UnAssignUserFromGroupsAsync(User user, PermissionGroup[] permissionGroups);
        Task UnAssignUserFromAllGroupsAsync(User user, string corp);

        Task<ICollection<Permission>> GetActiveRolesAsync(string user, string corp, string app, string env = null,
            string tenant = null);

        Task<ICollection<Permission>> GetMissingRolesAsync(string userId, (string, Verb)[] permissions, string corp,
            string app);

        Task UpdateLockStatusAsync(User user);
        Task UpdatePasswordAsync(User user);
    }

    public class DefaultUserService : DomainService<IUserRepository, Entities.User>, IUserService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly IPermissionGroupUserRepository _permissionGroupUserRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILocalUserInfoRepository _localUserInfoRepository;

        public DefaultUserService(IServiceProvider serviceProvider, IEncryptionService encryptionService,
            IPermissionGroupRepository permissionGroupRepository, IPermissionGroupUserRepository permissionGroupUserRepository,
            IRoleRepository roleRepository,
            ILocalUserInfoRepository localUserInfoRepository) :
            base(serviceProvider)
        {
            _encryptionService = encryptionService;
            _permissionGroupRepository = permissionGroupRepository;
            _permissionGroupUserRepository = permissionGroupUserRepository;
            _roleRepository = roleRepository;
            _localUserInfoRepository = localUserInfoRepository;
        }

        public User GetUser(string userId, string corp)
        {
            if (userId.IsBlank())
                throw new ArgumentNullException(nameof(userId));
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));

            var user = Repository.Find(userId);
            var localUserInfo = user?.UserInfos?.FirstOrDefault(x => x.Corp == corp);
            if (localUserInfo == null)
                return null;

            return new User
            {
                Id = userId,
                LocalUserInfos = new[]
                {
                    localUserInfo.ToDomainObject()
                },
                PermissionGroups = user.PermissionGroupUsers.OrEmpty()
                    .Where(x => x.PermissionGroup.Corp == corp)
                    .Select(x => new PermissionGroup
                    {
                        Name = x.PermissionGroup.Name,
                        Corp = corp,
                        App = x.PermissionGroup.App,
                        Locked = x.PermissionGroup.Locked,
                        Permissions = x.PermissionGroup.PermissionRecords.OrEmpty().Select(r => r.ToDomainObject()).ToArray()
                    }).ToArray()
            };
        }

        public async Task CreateUserAsync(User user, LocalUserInfo localUserInfo)
        {
            var encryptedPwd = localUserInfo.PlainPassword.IsBlank()
                ? null
                : _encryptionService.Encrypt(localUserInfo.PlainPassword);
            await Repository.CreateUserAsync(new Entities.User
            {
                Id = user.Id,
            }, new Entities.LocalUserInfo
            {
                Corp = localUserInfo.Corp,
                UserId = user.Id,
                Email = localUserInfo.Email,
                EncryptedPassword = encryptedPwd,
                Locked = localUserInfo.Locked,
            }.WithRandomId());
        }

        public async Task AssignUserToGroupsAsync(User user, PermissionGroup[] permissionGroups)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!permissionGroups.IsAny() || permissionGroups.Any(x => x == null))
                throw new ArgumentException(nameof(permissionGroups));

            if (permissionGroups.Select(g => $"{g.Corp}.{g.App}").Distinct().Count() > 1)
                throw new InvalidOperationException("Groups must belong to same application");

            var corp = permissionGroups.First().Corp;
            var app = permissionGroups.First().App;

            var lookupNames = permissionGroups.Select(x => x.Name).ToList();
            var lookupPermissionGroups =
                _permissionGroupRepository.FindMany(
                        new Expression<Func<Entities.PermissionGroup, bool>>[]
                        {
                            x => x.Corp == corp && x.App == app,
                            x => lookupNames.Contains(x.Name)
                        },
                        new FindOptions
                        {
                            Take = permissionGroups.Length
                        }
                    ).OrEmpty()
                    .ToArray();

            var missingPermissionGroups = permissionGroups
                .Select(x => (x.Name, x.Corp, x.App))
                .Except(lookupPermissionGroups.Select(x => (x.Name, x.Corp, x.App)))
                .ToArray();
            if (missingPermissionGroups.Any())
                throw new EntityNotExistsException(missingPermissionGroups.Select(g => g.Name));

            await Repository.AssignUserToGroups(new Entities.User
            {
                Id = user.Id
            }, lookupPermissionGroups);
        }

        public async Task UnAssignUserFromGroupsAsync(User user, PermissionGroup[] permissionGroups)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (permissionGroups == null)
                throw new ArgumentNullException(nameof(permissionGroups));

            if (permissionGroups.IsEmpty())
                return;

            if (permissionGroups.Any(x => x == null))
                throw new ArgumentException(nameof(permissionGroups));

            if (permissionGroups.Select(g => $"{g.Corp}.{g.App}").Distinct().Count() > 1)
                throw new InvalidOperationException("Groups must belong to same application");

            var corp = permissionGroups.First().Corp;
            var app = permissionGroups.First().App;

            var groupNames = permissionGroups.Select(x => x.Name).ToList();

            var tobeRemoved = _permissionGroupUserRepository.FindMany(new Expression<Func<PermissionGroupUser, bool>>[]
            {
                x => x.UserId == user.Id,
                x => x.PermissionGroup.Corp == corp && x.PermissionGroup.App == app,
                x => groupNames.Contains(x.PermissionGroup.Name)
            }, new FindOptions
            {
                Take = permissionGroups.Length
            }).OrEmpty().ToArray();

            if (tobeRemoved.Length != permissionGroups.Length)
                throw new EntityNotExistsException(permissionGroups.Select(g => g.Name)
                    .Except(tobeRemoved.Select(g => g.PermissionGroup.Name)));

            await Repository.UnAssignUserFromGroups(new Entities.User
            {
                Id = user.Id
            }, tobeRemoved.Select(x => x.PermissionGroup).ToArray());
        }

        public async Task UnAssignUserFromAllGroupsAsync(User user, string corp)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));

            var lookupUser = Repository.Find(user.Id);
            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            if (lookupUser.PermissionGroupUsers.IsEmpty())
                return;

            var tobeRemoved = lookupUser.PermissionGroupUsers.Where(rg => rg.PermissionGroup.Corp == corp).ToList();

            if (tobeRemoved.IsEmpty())
                return;

            await Repository.UnAssignUserFromGroups(new Entities.User
                {
                    Id = user.Id
                },
                tobeRemoved
                    .Select(x => x.PermissionGroup)
                    .ToArray()
            );
        }

        public async Task<ICollection<Permission>> GetActiveRolesAsync(string userId, string corp, string app,
            string env = null,
            string tenant = null)
        {
            var roleRecords = await FindRoleRecordsBasedOnFilterAsync(userId, corp, app, env, tenant);

            var roles = roleRecords
                .Select(x => x.ToDomainObject())
                .ToList();

            var roleIds = roles.Select(x => x.RoleId).ToHashSet();

            var lockedRoles = _roleRepository.FindMany(new Expression<Func<Entities.Role, bool>>[]
            {
                x =>
                    x.Corp == corp
                    &&
                    x.App == app
                    &&
                    x.Locked
                    &&
                    roleIds.Any(id => id == x.Id),
            }).OrEmpty().Select(x => x.Id).ToList();

            roles = roles.Where(x => !lockedRoles.Contains(x.RoleId)).ToList();
            roles = roles.DistinctRoles().ToList();
            roles = roles
                .Select(x => x.ToClientRoleModel())
                .DistinctPermissions()
                .Select(x => x.ToRole())
                .ToList();

            return roles;
        }

        public async Task<ICollection<Permission>> GetMissingRolesAsync(string userId, (string, Verb)[] permissions,
            string corp, string app)
        {
            if (permissions.Any(x => x.Item2 == Verb.None))
                throw new ArgumentException("Permission None is not a valid option");

            var activeRoles = await GetActiveRolesAsync(userId, corp, app);
            var userActiveClientRoleModels = activeRoles.Select(x => x.ToClientRoleModel());

            var requireClientRoleModels = permissions
                .SelectMany(x => RoleUtils.ParseToMinimum(x.Item1, x.Item2))
                .Select(x =>
                {
                    RoleUtils.Parse(x.Item1, out var requireClientRoleModel);
                    requireClientRoleModel.Verb = x.Item2;
                    return requireClientRoleModel;
                }).ToArray();

            var missing = requireClientRoleModels.Where(requireClientRoleModel =>
                !userActiveClientRoleModels.Any(activeRole =>
                    RoleUtils.ContainsOrEquals(activeRole, requireClientRoleModel, RoleUtils.ComparisionFlag.All))
            );

            return missing.Select(x => x.ToRole()).ToList();
        }

        private async Task<IEnumerable<PermissionRecord>> FindRoleRecordsBasedOnFilterAsync(string userId, string corp,
            string app, string env, string tenant)
        {
            if (userId.IsBlank())
                throw new ArgumentNullException(nameof(userId));

            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));

            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));

            if (Constants.WildCard.Equals(env))
                throw new ArgumentException($"{nameof(env)}: filter does not accept wildcard");

            if (Constants.WildCard.Equals(tenant))
                throw new ArgumentException($"{nameof(tenant)}: filter does not accept wildcard");

            var user = await Repository.FindAsync(userId);
            var localUserInfo = user?.UserInfos?.FirstOrDefault(x => x.Corp == corp);
            if (localUserInfo == null)
                throw new EntityNotExistsException($"{userId} at {corp}");

            var permissionGroups = user.PermissionGroupUsers
                .Where(x =>
                    x.PermissionGroup.Corp == corp
                    &&
                    x.PermissionGroup.App == app
                    &&
                    !x.PermissionGroup.Locked
                )
                .Select(x => x.PermissionGroup)
                .ToList();

            var permissionRecords = permissionGroups
                .SelectMany(x => x.PermissionRecords);

            if (!env.IsBlank())
                permissionRecords = permissionRecords.Where(rr => rr.Env == Constants.WildCard || rr.Env == env);

            if (!tenant.IsBlank())
                permissionRecords = permissionRecords.Where(rr => rr.Tenant == Constants.WildCard || rr.Tenant == tenant);

            return permissionRecords;
        }

        public async Task UpdateLockStatusAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!user.LocalUserInfos.IsAny())
                throw new ArgumentNullException($"Require {nameof(user.LocalUserInfos)} of {nameof(user)}");

            if (user.LocalUserInfos.Any(x => x == null))
                throw new ArgumentException($"{nameof(user.LocalUserInfos)} of {nameof(user)} contains null");

            var lookupUser = Repository.Find(user.Id);

            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            if (!lookupUser.UserInfos.IsAny())
                return;

            var tobeUpdated = new List<Entities.LocalUserInfo>();

            foreach (var localUserInfo in user.LocalUserInfos)
            {
                var lookupUserUserInfo = lookupUser.UserInfos.FirstOrDefault(x => x.Corp == localUserInfo.Corp);
                if (lookupUserUserInfo == null)
                    throw new EntityNotExistsException($"{user.Id} at {localUserInfo.Corp}");

                if (lookupUserUserInfo.Locked == localUserInfo.Locked)
                    continue;

                lookupUserUserInfo.Locked = localUserInfo.Locked;

                tobeUpdated.Add(lookupUserUserInfo);
            }

            if (!tobeUpdated.Any())
                return;

            await _localUserInfoRepository.UpdateManyAsync(tobeUpdated);
        }

        public async Task UpdatePasswordAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!user.LocalUserInfos.IsAny())
                throw new ArgumentNullException($"Require {nameof(user.LocalUserInfos)} of {nameof(user)}");

            if (user.LocalUserInfos.Any(x => x == null))
                throw new ArgumentException($"{nameof(user.LocalUserInfos)} of {nameof(user)} contains null");

            var lookupUser = Repository.Find(user.Id);

            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            if (!lookupUser.UserInfos.IsAny())
                return;

            var tobeUpdated = new List<Entities.LocalUserInfo>();

            foreach (var localUserInfo in user.LocalUserInfos)
            {
                var lookupUserUserInfo = lookupUser.UserInfos?.FirstOrDefault(x => x.Corp == localUserInfo.Corp);
                if (lookupUserUserInfo == null)
                    throw new EntityNotExistsException($"{user.Id} at {localUserInfo.Corp}");

                lookupUserUserInfo.EncryptedPassword = localUserInfo.PlainPassword.IsBlank()
                    ? null
                    : _encryptionService.Encrypt(localUserInfo.PlainPassword);

                tobeUpdated.Add(lookupUserUserInfo);
            }

            await _localUserInfoRepository.UpdateManyAsync(tobeUpdated);
        }
    }
}