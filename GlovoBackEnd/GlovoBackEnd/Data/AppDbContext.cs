using GlovoBackEnd.Domain.Entities;
using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Domain.Entities.Company;
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

    public DbSet<PartnerEntity> Partners => Set<PartnerEntity>();
    public DbSet<PartnerRoleEntity> PartnerRoles => Set<PartnerRoleEntity>();
    public DbSet<PartnerUserRoleEntity> PartnerUserRoles => Set<PartnerUserRoleEntity>();
    public DbSet<CompanyEntity> Companies => Set<CompanyEntity>();

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

        modelBuilder.Entity<PartnerEntity>(partner =>
        {
            partner.ToTable("Partners");
            partner.HasIndex(x => x.NormalizedEmail).IsUnique();
            partner.HasIndex(x => x.Phone).IsUnique();
            partner.Property(x => x.Email).HasMaxLength(254).IsRequired();
            partner.Property(x => x.NormalizedEmail).HasMaxLength(254).IsRequired();
            partner.Property(x => x.Phone).HasMaxLength(32).IsRequired();
            partner.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            partner.Property(x => x.LastName).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<PartnerRoleEntity>(role =>
        {
            role.ToTable("PartnerRoles");
            role.HasIndex(x => x.Name).IsUnique();
            role.Property(x => x.Name).HasMaxLength(64).IsRequired();
            role.HasData(
                new PartnerRoleEntity { Id = 1, Name = PartnerRoleNames.CompanyOwner },
                new PartnerRoleEntity { Id = 2, Name = PartnerRoleNames.AffiliateManager },
                new PartnerRoleEntity { Id = 3, Name = PartnerRoleNames.AffiliateEmployee },
                new PartnerRoleEntity { Id = 4, Name = PartnerRoleNames.User });
        });

        modelBuilder.Entity<PartnerUserRoleEntity>(partnerRole =>
        {
            partnerRole.ToTable("PartnerUserRoles");
            partnerRole.HasKey(x => new { x.PartnerId, x.RoleId });
            partnerRole.HasOne(x => x.Partner)
                .WithMany(x => x.PartnerRoles)
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
            partnerRole.HasOne(x => x.Role)
                .WithMany(x => x.PartnerUsers)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CompanyEntity>(company =>
        {
            company.ToTable("Companies");
            company.HasIndex(x => new { x.OwnerId, x.NormalizedName }).IsUnique();
            company.Property(x => x.Name).HasMaxLength(120).IsRequired();
            company.Property(x => x.NormalizedName).HasMaxLength(120).IsRequired();
            company.Property(x => x.Description).HasMaxLength(1_000).IsRequired();
            company.HasOne(x => x.Owner)
                .WithMany(x => x.Companies)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
