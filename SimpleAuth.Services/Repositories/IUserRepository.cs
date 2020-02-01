using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Repositories
{
    public interface IUserRepository : IRepository<User, string>
    {
        Task CreateUserAsync(User user, LocalUserInfo userInfo);
        Task AssignUserToGroups(User user, RoleGroup[] roleGroups);
        Task UnAssignUserFromGroups(User user, RoleGroup[] roleGroups);
    }

    public class UserRepository : Repository<User, string>, IUserRepository
    {
        private readonly IRoleGroupRepository _roleGroupRepository;

        public UserRepository(IDbContextFactory dbContextFactory,
            IRoleGroupRepository roleGroupRepository)
            : base(dbContextFactory)
        {
            _roleGroupRepository = roleGroupRepository;
        }

        protected override IQueryable<User> Include(DbSet<User> dbSet)
        {
            return base
                .Include(dbSet)
                .Include(x => x.RoleGroupUsers)
                .ThenInclude(gu => gu.RoleGroup)
                .ThenInclude(rg => rg.RoleRecords)
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

        public async Task AssignUserToGroups(User user, RoleGroup[] roleGroups)
        {
            var targetGrIds = roleGroups.Select(rg => rg.Id);
            var lookupRoleGroups = _roleGroupRepository
                .Find(x => targetGrIds.Contains(x.Id)
                ).ToArray();

            if (lookupRoleGroups.Length != roleGroups.Length)
                throw new EntityNotExistsException(roleGroups.Select(g => g.Name)
                    .Except(lookupRoleGroups.Select(g => g.Name)));

            await using var ctx = OpenConnect();

            var lookupUser = await ctx.Set<User>()
                .Include(x => x.RoleGroupUsers)
                .FirstOrDefaultAsync(x => x.Id == user.Id);
            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            var toBeAdded = lookupUser.RoleGroupUsers.IsEmpty()
                ? lookupRoleGroups
                : lookupRoleGroups
                    .Where(g => lookupUser.RoleGroupUsers.Any(x => x.RoleGroupId != g.Id))
                    .ToArray();

            if (toBeAdded.IsEmpty())
                return;

            foreach (var gr in toBeAdded)
            {
                var newRecord = new RoleGroupUser
                {
                    UserId = lookupUser.Id,
                    RoleGroupId = gr.Id,
                };
                ctx.Set<RoleGroupUser>().Add(newRecord);

                lookupUser.RoleGroupUsers = lookupUser.RoleGroupUsers.Concat(newRecord).ToList();

                gr.RoleGroupUsers = gr.RoleGroupUsers.Concat(newRecord).ToList();
                ctx.Set<RoleGroup>().Update(gr);
            }

            ctx.Set<User>().Update(lookupUser);

            await ctx.SaveChangesAsync();
        }

        public async Task UnAssignUserFromGroups(User user, RoleGroup[] roleGroups)
        {
            var targetGrIds = roleGroups.Select(rg => rg.Id);
            var lookupRoleGroups = _roleGroupRepository
                .Find(x => targetGrIds.Contains(x.Id)
                ).ToArray();

            if (lookupRoleGroups.Length != roleGroups.Length)
                throw new EntityNotExistsException(roleGroups.Select(g => g.Name)
                    .Except(lookupRoleGroups.Select(g => g.Name)));

            await using var ctx = OpenConnect();

            var lookupUser = await ctx.Set<User>()
                .Include(x => x.RoleGroupUsers)
                .FirstOrDefaultAsync(x => x.Id == user.Id);
            if (lookupUser == null)
                throw new EntityNotExistsException(user.Id);

            if (lookupUser.RoleGroupUsers.IsEmpty())
                return;

            var rguSet = ctx.Set<RoleGroupUser>();
            foreach (var gr in lookupRoleGroups)
            {
                gr.RoleGroupUsers.Remove(gr.RoleGroupUsers.First(x =>
                    x.RoleGroupId == gr.Id && x.UserId == lookupUser.Id));
                lookupUser.RoleGroupUsers.Remove(
                    lookupUser.RoleGroupUsers.First(x => x.RoleGroupId == gr.Id && x.UserId == lookupUser.Id));

                ctx.Set<RoleGroup>().Update(gr);
                ctx.Set<User>().Update(lookupUser);

                rguSet.Remove(rguSet.Find(lookupUser.Id, gr.Id));
            }

            await ctx.SaveChangesAsync();
        }
    }
}