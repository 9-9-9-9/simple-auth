using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services.Entities;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.BuildIndexesFromAnnotations();
            modelBuilder.Entity<RoleGroup>().HasIndex(rg => new {rg.Name, rg.Corp, rg.App}).IsUnique();
            modelBuilder.Entity<LocalUserInfo>().HasIndex(info => new {info.UserId, info.Corp}).IsUnique();
            modelBuilder.Entity<LocalUserInfo>().HasIndex(info => new {info.NormalizedEmail, info.Corp}).IsUnique();
            modelBuilder.Entity<TokenInfo>().HasIndex(info => new {info.Corp, info.App}).IsUnique();

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
    }
}