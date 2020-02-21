using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Repositories
{
    public interface IUserRepository : IRepository<User, string>
    {
        Task CreateUserAsync(User user, LocalUserInfo userInfo);
        Task AssignUserToGroups(User user, PermissionGroup[] permissionGroups);
        Task UnAssignUserFromGroups(User user, PermissionGroup[] permissionGroups);
    }

    public class UserRepository : Repository<User, string>, IUserRepository
    {
        private readonly IPermissionGroupRepository _permissionGroupRepository;

        public UserRepository(IDbContextFactory dbContextFactory,
            IPermissionGroupRepository permissionGroupRepository)
            : base(dbContextFactory)
        {
            _permissionGroupRepository = permissionGroupRepository;
        }

        protected override IQueryable<User> Include(DbSet<User> dbSet)
        {
            return base
                .Include(dbSet)
                .Include(x => x.PermissionGroupUsers)
                .ThenInclude(gu => gu.PermissionGroup)
                .ThenInclude(rg => rg.PermissionRecords)
                .Include(x => x.UserInfos);
        }

#pragma warning disable 1998
        public override async Task<int> DeleteManyAsync(IEnumerable<User> entities)
        {
            throw new NotSupportedException("Records in User table is for sharing, thus don't delete it'");
        }
#pragma warning restore 1998

        public async Task CreateUserAsync(User user, LocalUserInfo userInfo)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (userInfo == null)
                throw new ArgumentNullException(nameof(userInfo));
            
            var lookupUser = Find(user.Id);

            await using var ctx = OpenConnect();
            var dbUsers = ctx.Set<User>();
            var persist = false;
            if (lookupUser == null)
            {
                user.NormalizedId = user.Id.NormalizeInput();
                ctx.Entry(user).State = EntityState.Added;
                persist = true;
                lookupUser = user;
            }

            if (true == lookupUser.UserInfos?.Any(info => info.Corp == userInfo.Corp))
            {
                throw new EntityAlreadyExistsException($"{user.Id} at {userInfo.Corp}");
            }

            var newUserInfo = new LocalUserInfo
            {
                UserId = user.Id,
                Email = userInfo.Email,
                NormalizedEmail = userInfo.Email,
                Corp = userInfo.Corp,
                Locked = userInfo.Locked,
                EncryptedPassword = userInfo.EncryptedPassword
            }.WithRandomId();

            var dbUserInfos = ctx.Set<LocalUserInfo>();
            ctx.Entry(newUserInfo).State = EntityState.Added;
            await dbUserInfos.AddAsync(newUserInfo);

            // ReSharper disable once PossibleNullReferenceException
            lookupUser.UserInfos.Add(newUserInfo);
            if (persist)
                await dbUsers.AddAsync(lookupUser);
            else
                dbUsers.Update(lookupUser);

            await ctx.SaveChangesAsync();
        }

        public async Task AssignUserToGroups(User user, PermissionGroup[] permissionGroups)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (permissionGroups.IsEmpty())
                throw new ArgumentNullException(nameof(permissionGroups));
            
            var targetGrIds = permissionGroups.Select(rg => rg.Id);
            var lookupPermissionGroups = _permissionGroupRepository
                .Find(x => targetGrIds.Contains(x.Id)
                ).ToArray();

            if (lookupPermissionGroups.Length != permissionGroups.Length)
                throw new EntityNotExistsException(permissionGroups.Select(g => g.Name)
                    .Except(lookupPermissionGroups.Select(g => g.Name)));

            await using var ctx = OpenConnect();

            var lookupUser = await ctx.Set<User>()
                .Include(x => x.PermissionGroupUsers)
                .FirstOrDefaultAsync(x => x.Id == user.Id);
            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            var toBeAdded = lookupUser.PermissionGroupUsers.IsEmpty()
                ? lookupPermissionGroups
                : lookupPermissionGroups
                    .Where(g => lookupUser.PermissionGroupUsers.Any(x => x.PermissionGroupId != g.Id))
                    .ToArray();

            if (toBeAdded.IsEmpty())
                return;

            foreach (var gr in toBeAdded)
            {
                var newRecord = new PermissionGroupUser
                {
                    UserId = lookupUser.Id,
                    PermissionGroupId = gr.Id,
                };
                ctx.Set<PermissionGroupUser>().Add(newRecord);

                lookupUser.PermissionGroupUsers = lookupUser.PermissionGroupUsers.Concat(newRecord).ToList();

                gr.PermissionGroupUsers = gr.PermissionGroupUsers.Concat(newRecord).ToList();
                ctx.Set<PermissionGroup>().Update(gr);
            }

            ctx.Set<User>().Update(lookupUser);

            await ctx.SaveChangesAsync();
        }

        public async Task UnAssignUserFromGroups(User user, PermissionGroup[] permissionGroups)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (permissionGroups.IsEmpty())
                throw new ArgumentNullException(nameof(permissionGroups));
            
            var targetGrIds = permissionGroups.Select(rg => rg.Id);
            var lookupPermissionGroups = _permissionGroupRepository
                .Find(x => targetGrIds.Contains(x.Id)
                ).ToArray();

            if (lookupPermissionGroups.Length != permissionGroups.Length)
                throw new EntityNotExistsException(permissionGroups.Select(g => g.Name)
                    .Except(lookupPermissionGroups.Select(g => g.Name)));

            await using var ctx = OpenConnect();

            var lookupUser = await ctx.Set<User>()
                .Include(x => x.PermissionGroupUsers)
                .FirstOrDefaultAsync(x => x.Id == user.Id);
            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            if (lookupUser.PermissionGroupUsers.IsEmpty())
                return;

            var rguSet = ctx.Set<PermissionGroupUser>();
            foreach (var gr in lookupPermissionGroups)
            {
                gr.PermissionGroupUsers.Remove(gr.PermissionGroupUsers.First(x =>
                    x.PermissionGroupId == gr.Id && x.UserId == lookupUser.Id));
                lookupUser.PermissionGroupUsers.Remove(
                    lookupUser.PermissionGroupUsers.First(x => x.PermissionGroupId == gr.Id && x.UserId == lookupUser.Id));

                ctx.Set<PermissionGroup>().Update(gr);
                ctx.Set<User>().Update(lookupUser);

                rguSet.Remove(rguSet.Find(lookupUser.Id, gr.Id));
            }

            await ctx.SaveChangesAsync();
        }
    }
}