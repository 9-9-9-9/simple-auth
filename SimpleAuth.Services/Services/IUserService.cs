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
using Role = SimpleAuth.Shared.Domains.Role;
using RoleGroup = SimpleAuth.Shared.Domains.RoleGroup;
using User = SimpleAuth.Shared.Domains.User;

namespace SimpleAuth.Services
{
    public interface IUserService : IDomainService
    {
        User GetUser(string userId, string corp);
        Task CreateUserAsync(User user, LocalUserInfo localUserInfo);
        Task AssignUserToGroupsAsync(User user, RoleGroup[] roleGroups);
        Task UnAssignUserFromGroupsAsync(User user, RoleGroup[] roleGroups);
        Task UnAssignUserFromAllGroupsAsync(User user, string corp);

        Task<ICollection<Role>> GetActiveRolesAsync(string user, string corp, string app, string env = null,
            string tenant = null);

        Task<ICollection<Role>> GetMissingRolesAsync(string userId, (string, Permission)[] permissions, string corp,
            string app);

        Task UpdateLockStatusAsync(User user);
        Task UpdatePasswordAsync(User user);
    }

    public class DefaultUserService : DomainService<IUserRepository, Entities.User>, IUserService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IRoleGroupRepository _roleGroupRepository;
        private readonly IRoleGroupUserRepository _roleGroupUserRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILocalUserInfoRepository _localUserInfoRepository;

        public DefaultUserService(IServiceProvider serviceProvider, IEncryptionService encryptionService,
            IRoleGroupRepository roleGroupRepository, IRoleGroupUserRepository roleGroupUserRepository,
            IRoleRepository roleRepository,
            ILocalUserInfoRepository localUserInfoRepository) :
            base(serviceProvider)
        {
            _encryptionService = encryptionService;
            _roleGroupRepository = roleGroupRepository;
            _roleGroupUserRepository = roleGroupUserRepository;
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
                RoleGroups = user.RoleGroupUsers.OrEmpty()
                    .Where(x => x.RoleGroup.Corp == corp)
                    .Select(x => new RoleGroup
                    {
                        Name = x.RoleGroup.Name,
                        Corp = corp,
                        App = x.RoleGroup.App,
                        Locked = x.RoleGroup.Locked,
                        Roles = x.RoleGroup.RoleRecords.OrEmpty().Select(r => r.ToDomainObject()).ToArray()
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

        public async Task AssignUserToGroupsAsync(User user, RoleGroup[] roleGroups)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (!roleGroups.IsAny() || roleGroups.Any(x => x == null))
                throw new ArgumentException(nameof(roleGroups));
            
            if (roleGroups.Select(g => $"{g.Corp}.{g.App}").Distinct().Count() > 1)
                throw new InvalidOperationException($"Groups must belong to same application");

            var corp = roleGroups.First().Corp;
            var app = roleGroups.First().App;

            var lookupNames = roleGroups.Select(x => x.Name).ToList();
            var lookupRoleGroups =
                _roleGroupRepository.FindMany(
                        new Expression<Func<Entities.RoleGroup, bool>>[]
                        {
                            x => x.Corp == corp && x.App == app,
                            x => lookupNames.Contains(x.Name)
                        },
                        new FindOptions
                        {
                            Take = roleGroups.Length
                        }
                    ).OrEmpty()
                    .ToArray();

            var missingRoleGroups = roleGroups
                .Select(x => (x.Name, x.Corp, x.App))
                .Except(lookupRoleGroups.Select(x => (x.Name, x.Corp, x.App)))
                .ToArray();
            if (missingRoleGroups.Any())
                throw new EntityNotExistsException(missingRoleGroups.Select(g => g.Name));

            await Repository.AssignUserToGroups(new Entities.User
            {
                Id = user.Id
            }, lookupRoleGroups);
        }

        public async Task UnAssignUserFromGroupsAsync(User user, RoleGroup[] roleGroups)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (roleGroups == null)
                throw new ArgumentNullException(nameof(roleGroups));

            if (roleGroups.IsEmpty())
                return;
            
            if (roleGroups.Any(x => x == null))
                throw new ArgumentException(nameof(roleGroups));
            
            if (roleGroups.Select(g => $"{g.Corp}.{g.App}").Distinct().Count() > 1)
                throw new InvalidOperationException($"Groups must belong to same application");

            var corp = roleGroups.First().Corp;
            var app = roleGroups.First().App;

            var groupNames = roleGroups.Select(x => x.Name).ToList();

            var tobeRemoved = _roleGroupUserRepository.FindMany(new Expression<Func<RoleGroupUser, bool>>[]
            {
                x => x.UserId == user.Id,
                x => x.RoleGroup.Corp == corp && x.RoleGroup.App == app,
                x => groupNames.Contains(x.RoleGroup.Name)
            }, new FindOptions
            {
                Take = roleGroups.Length
            }).OrEmpty().ToArray();

            if (tobeRemoved.Length != roleGroups.Length)
                throw new EntityNotExistsException(roleGroups.Select(g => g.Name)
                    .Except(tobeRemoved.Select(g => g.RoleGroup.Name)));

            await Repository.UnAssignUserFromGroups(new Entities.User
            {
                Id = user.Id
            }, tobeRemoved.Select(x => x.RoleGroup).ToArray());
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

            if (lookupUser.RoleGroupUsers.IsEmpty())
                return;

            var tobeRemoved = lookupUser.RoleGroupUsers.Where(rg => rg.RoleGroup.Corp == corp).ToList();

            if (tobeRemoved.IsEmpty())
                return;

            await Repository.UnAssignUserFromGroups(new Entities.User
                {
                    Id = user.Id
                },
                tobeRemoved
                    .Select(x => x.RoleGroup)
                    .ToArray()
            );
        }

        public async Task<ICollection<Role>> GetActiveRolesAsync(string userId, string corp, string app,
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
            }).Select(x => x.Id).ToList();

            roles = roles
                .Where(x => !lockedRoles.Contains(x.RoleId))
                .DistinctRoles()
                .Select(x => x.ToClientRoleModel())
                .DistinctRoles()
                .Select(x => x.ToRole())
                .ToList();

            return roles;
        }

        public async Task<ICollection<Role>> GetMissingRolesAsync(string userId, (string, Permission)[] permissions,
            string corp, string app)
        {
            if (permissions.Any(x => x.Item2 == Permission.None))
                throw new ArgumentException("Permission None is not a valid option");

            var activeRoles = await GetActiveRolesAsync(userId, corp, app);
            var userActiveClientRoleModels = activeRoles.Select(x => x.ToClientRoleModel());

            var requireClientRoleModels = permissions.Select(x =>
            {
                RoleUtils.Parse(x.Item1, out var requireClientRoleModel);
                requireClientRoleModel.Permission = x.Item2;
                return requireClientRoleModel;
            }).ToArray();

            var missing = requireClientRoleModels.Where(requireClientRoleModel =>
                !userActiveClientRoleModels.Any(activeRole =>
                    RoleUtils.ContainsOrEquals(activeRole, requireClientRoleModel, RoleUtils.ComparisionFlag.All))
            );

            return missing.Select(x => x.ToRole()).ToList();
        }

        private async Task<IEnumerable<RoleRecord>> FindRoleRecordsBasedOnFilterAsync(string userId, string corp,
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

            var roleGroups = user.RoleGroupUsers
                .Where(x =>
                    x.RoleGroup.Corp == corp
                    &&
                    x.RoleGroup.App == app
                    &&
                    !x.RoleGroup.Locked
                )
                .Select(x => x.RoleGroup)
                .ToList();

            var roleRecords = roleGroups
                .SelectMany(x => x.RoleRecords);

            if (!env.IsBlank())
                roleRecords = roleRecords.Where(rr => rr.Env == Constants.WildCard || rr.Env == env);

            if (!tenant.IsBlank())
                roleRecords = roleRecords.Where(rr => rr.Tenant == Constants.WildCard || rr.Tenant == tenant);

            return roleRecords;
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