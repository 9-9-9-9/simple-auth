using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations;

namespace SimpleAuth.Repositories
{
    public abstract class SimpleAuthDbContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleGroup> RoleGroups { get; set; }
        public DbSet<RoleRecord> RoleRecords { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LocalUserInfo> LocalUserInfos { get; set; }
        public DbSet<RoleGroupUser> RoleGroupUsers { get; set; }
        public DbSet<TokenInfo> TokenInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.BuildIndexesFromAnnotations();
            modelBuilder.Entity<RoleGroup>().HasIndex(rg => new {rg.Name, rg.Corp, rg.App}).IsUnique();
            modelBuilder.Entity<LocalUserInfo>().HasIndex(info => new {info.UserId, info.Corp}).IsUnique();
            modelBuilder.Entity<LocalUserInfo>().HasIndex(info => new {info.NormalizedEmail, info.Corp}).IsUnique();
            modelBuilder.Entity<TokenInfo>().HasIndex(info => new {info.Corp, info.App}).IsUnique();

            RegisterIndex<LocalUserInfo>(modelBuilder);
            RegisterIndex<Role>(modelBuilder);
            RegisterIndex<RoleGroup>(modelBuilder);
            RegisterIndex<RoleGroupUser>(modelBuilder);
            RegisterIndex<RoleRecord>(modelBuilder);
            RegisterIndex<TokenInfo>(modelBuilder);
            RegisterIndex<User>(modelBuilder);

            modelBuilder.Entity<RoleGroupUser>()
                .HasKey(bc => new {bc.UserId, bc.RoleGroupId});
            modelBuilder.Entity<RoleGroupUser>()
                .HasOne(bc => bc.User)
                .WithMany(b => b.RoleGroupUsers)
                .HasForeignKey(bc => bc.UserId);
            modelBuilder.Entity<RoleGroupUser>()
                .HasOne(bc => bc.RoleGroup)
                .WithMany(c => c.RoleGroupUsers)
                .HasForeignKey(bc => bc.RoleGroupId);
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
                modelBuilder.Entity<T>().HasIndex(x => (x as IPermissionRelated).Permission);
            if (typeof(ILockable).IsAssignableFrom(typeof(T)))
                modelBuilder.Entity<T>().HasIndex(x => (x as ILockable).Locked);
        }
    }
}