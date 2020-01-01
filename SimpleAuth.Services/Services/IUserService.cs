using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Exceptions;
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
        Task AssignUserToGroups(User user, RoleGroup[] roleGroups);
        Task UnAssignUserFromGroups(User user, RoleGroup[] roleGroups);
        Task UnAssignUserFromAllGroups(User user, string corp);
        ICollection<Role> GetActiveRoles(string user, string corp, string app);
        Task UpdateLockStatusAsync(User user);
        Task UpdatePasswordAsync(User user);
    }

    public class DefaultUserService : DomainService<IUserRepository, Entities.User>, IUserService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IRoleGroupRepository _roleGroupRepository;
        private readonly IRoleGroupUserRepository _roleGroupUserRepository;
        private readonly ICachedUserRolesRepository _cachedUserRolesRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILocalUserInfoRepository _localUserInfoRepository;

        public DefaultUserService(IServiceProvider serviceProvider, IEncryptionService encryptionService,
            IRoleGroupRepository roleGroupRepository, IRoleGroupUserRepository roleGroupUserRepository,
            ICachedUserRolesRepository cachedUserRolesRepository, IRoleRepository roleRepository,
            ILocalUserInfoRepository localUserInfoRepository) : base(
            serviceProvider)
        {
            _encryptionService = encryptionService;
            _roleGroupRepository = roleGroupRepository;
            _roleGroupUserRepository = roleGroupUserRepository;
            _cachedUserRolesRepository = cachedUserRolesRepository;
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

        public async Task AssignUserToGroups(User user, RoleGroup[] roleGroups)
        {
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
                    )
                    .ToArray();

            var missingRoleGroups = roleGroups
                .Where(g => lookupRoleGroups.All(lg =>
                    lg.Name != g.Name
                    &&
                    lg.Corp != g.Corp
                    &&
                    lg.App != g.App)).ToArray();
            if (missingRoleGroups.Any())
                throw new EntityNotExistsException(missingRoleGroups.Select(g => g.Name));

            _cachedUserRolesRepository.Clear(corp, app);

            await Repository.AssignUserToGroups(new Entities.User
            {
                Id = user.Id
            }, lookupRoleGroups);
        }

        public async Task UnAssignUserFromGroups(User user, RoleGroup[] roleGroups)
        {
            if (roleGroups.IsEmpty())
                return;

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
            }).ToArray();

            if (tobeRemoved.Length != roleGroups.Length)
                throw new EntityNotExistsException(roleGroups.Select(g => g.Name)
                    .Except(tobeRemoved.Select(g => g.RoleGroup.Name)));

            _cachedUserRolesRepository.Clear(corp, app);

            await Repository.UnAssignUserFromGroups(new Entities.User
            {
                Id = user.Id
            }, tobeRemoved.Select(x => x.RoleGroup).ToArray());
        }

        public async Task UnAssignUserFromAllGroups(User user, string corp)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));

            var lookupUser = Repository.Find(user.Id);
            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            if (lookupUser.RoleGroupUsers.IsEmpty())
                return;

            var tobeRemoved = lookupUser.RoleGroupUsers.Where(rg => rg.RoleGroup.Corp == corp).ToList();

            tobeRemoved.Select(x => (x.RoleGroup.Corp, x.RoleGroup.App)).Distinct().ToList().ForEach(x =>
            {
                _cachedUserRolesRepository.Clear(x.Corp, x.App);
            });

            await Repository.UnAssignUserFromGroups(new Entities.User
                {
                    Id = user.Id
                },
                tobeRemoved
                    .Select(x => x.RoleGroup)
                    .ToArray()
            );
        }

        public ICollection<Role> GetActiveRoles(string userId, string corp, string app)
        {
            if (userId.IsBlank())
                throw new ArgumentNullException(nameof(userId));

            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));

            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));

            var cachedRoles = _cachedUserRolesRepository.Get(userId, corp, app) as List<Role>;
            if (cachedRoles.IsAny())
                return cachedRoles;

            var user = Repository.Find(userId);
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

            var roles = roleGroups
                .SelectMany(x => x.RoleRecords)
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
                .ToList();

            _cachedUserRolesRepository.Push(roles, userId, corp, app);

            return roles;
        }

        public async Task UpdateLockStatusAsync(User user)
        {
            var lookupUser = Repository.Find(user.Id);

            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            var tobeUpdated = new List<Entities.LocalUserInfo>();

            foreach (var localUserInfo in user.LocalUserInfos)
            {
                var lookupUserUserInfo = lookupUser.UserInfos?.FirstOrDefault(x => x.Corp == localUserInfo.Corp);
                if (lookupUserUserInfo == null)
                    throw new EntityNotExistsException($"{user.Id} at {localUserInfo.Corp}");

                lookupUserUserInfo.Locked = localUserInfo.Locked;

                tobeUpdated.Add(lookupUserUserInfo);
            }

            await _localUserInfoRepository.UpdateManyAsync(tobeUpdated);
        }

        public async Task UpdatePasswordAsync(User user)
        {
            var lookupUser = Repository.Find(user.Id);

            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

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