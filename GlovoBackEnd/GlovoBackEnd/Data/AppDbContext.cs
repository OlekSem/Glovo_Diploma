using GlovoBackEnd.Domain.Entities;
using GlovoBackEnd.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity; // Contains your ApplicationUser & ApplicationRole
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Data;

// 1. Inherit from IdentityDbContext and pass your custom User and Role entities
public class AppDbContext : IdentityDbContext<
    UserEntity, 
    RoleEntity, 
    Guid, 
    IdentityUserClaim<Guid>, 
    UserRoleEntity, 
    IdentityUserLogin<Guid>, 
    IdentityRoleClaim<Guid>, 
    IdentityUserToken<Guid>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure custom UserRole relationship if needed
        modelBuilder.Entity<UserRoleEntity>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            userRole.HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });
    }
}