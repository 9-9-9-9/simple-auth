using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations;

namespace SimpleAuth.Repositories
{
    public abstract class SimpleAuthDbContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<PermissionRecord> PermissionRecords { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LocalUserInfo> LocalUserInfos { get; set; }
        public DbSet<PermissionGroupUser> PermissionGroupUsers { get; set; }
        public DbSet<TokenInfo> TokenInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.BuildIndexesFromAnnotations();
            modelBuilder.Entity<PermissionGroup>().HasIndex(rg => new {rg.Name, rg.Corp, rg.App}).IsUnique();
            modelBuilder.Entity<LocalUserInfo>().HasIndex(info => new {info.UserId, info.Corp}).IsUnique();
            modelBuilder.Entity<LocalUserInfo>().HasIndex(info => new {info.NormalizedEmail, info.Corp}).IsUnique();
            modelBuilder.Entity<TokenInfo>().HasIndex(info => new {info.Corp, info.App}).IsUnique();

            RegisterIndex<LocalUserInfo>(modelBuilder);
            RegisterIndex<Role>(modelBuilder);
            RegisterIndex<PermissionGroup>(modelBuilder);
            RegisterIndex<PermissionGroupUser>(modelBuilder);
            RegisterIndex<PermissionRecord>(modelBuilder);
            RegisterIndex<TokenInfo>(modelBuilder);
            RegisterIndex<User>(modelBuilder);

            modelBuilder.Entity<PermissionGroupUser>()
                .HasKey(bc => new {bc.UserId, RoleGroupId = bc.PermissionGroupId});
            modelBuilder.Entity<PermissionGroupUser>()
                .HasOne(bc => bc.User)
                .WithMany(b => b.PermissionGroupUsers)
                .HasForeignKey(bc => bc.UserId);
            modelBuilder.Entity<PermissionGroupUser>()
                .HasOne(bc => bc.PermissionGroup)
                .WithMany(c => c.PermissionGroupUsers)
                .HasForeignKey(bc => bc.PermissionGroupId);
        }

        private void RegisterIndex<T>(ModelBuilder modelBuilder) where T : class
        {
            if (typeof(ICorpRelated).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as ICorpRelated).Corp);
            if (typeof(IAppRelated).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as IAppRelated).App);
            if (typeof(IEnvRelated).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as IEnvRelated).Env);
            if (typeof(ITenantRelated).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as ITenantRelated).Tenant);
            if (typeof(IModuleRelated).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as IModuleRelated).Module);
            if (typeof(ISubModuleRelated).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as ISubModuleRelated).SubModules);
            if (typeof(IPermissionRelated).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as IPermissionRelated).Verb);
            if (typeof(ILockable).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as ILockable).Locked);
        }
    }
}