using GlovoBackEnd.Domain.Entities;
using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Domain.Enums;
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
    public DbSet<CompanyBranchEntity> CompanyBranches => Set<CompanyBranchEntity>();
    public DbSet<CompanyProductCategoryEntity> CompanyProductCategories => Set<CompanyProductCategoryEntity>();
    public DbSet<CompanyProductEntity> CompanyProducts => Set<CompanyProductEntity>();
    public DbSet<ProductAddonGroupEntity> ProductAddonGroups => Set<ProductAddonGroupEntity>();
    public DbSet<ProductAddonItemEntity> ProductAddonItems => Set<ProductAddonItemEntity>();
    public DbSet<BranchProductCategoryEntity> BranchProductCategories => Set<BranchProductCategoryEntity>();
    public DbSet<BranchProductEntity> BranchProducts => Set<BranchProductEntity>();

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
            company.Property(x => x.Category).HasMaxLength(120);
            company.Property(x => x.Subcategory).HasMaxLength(120);
            company.Property(x => x.IconUrl).HasMaxLength(2_000);
            company.Property(x => x.BannerUrl).HasMaxLength(2_000);
            company.Property(x => x.Status).HasConversion<string>().HasMaxLength(16).HasDefaultValue(CompanyStatus.Pending).IsRequired();
            company.Property(x => x.ModerationComment).HasMaxLength(1_000);
            company.HasIndex(x => x.Status);
            company.HasOne(x => x.Owner)
                .WithMany(x => x.Companies)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CompanyBranchEntity>(branch =>
        {
            branch.ToTable("CompanyBranches");
            branch.Property(x => x.Phone).HasMaxLength(32).IsRequired();
            branch.Property(x => x.Email).HasMaxLength(254).IsRequired();
            branch.Property(x => x.Locality).HasMaxLength(120).IsRequired();
            branch.Property(x => x.Region).HasMaxLength(120).IsRequired();
            branch.Property(x => x.Address).HasMaxLength(250).IsRequired();
            branch.Property(x => x.PostalCode).HasMaxLength(12).IsRequired();
            branch.HasIndex(x => x.CompanyId);
            branch.HasOne(x => x.Company)
                .WithMany(x => x.Branches)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CompanyProductCategoryEntity>(category =>
        {
            category.ToTable("CompanyProductCategories");
            category.Property(x => x.Name).HasMaxLength(120).IsRequired();
            category.Property(x => x.NormalizedName).HasMaxLength(120).IsRequired();
            category.Property(x => x.SortOrder).HasDefaultValue(0);
            category.HasIndex(x => new { x.CompanyId, x.NormalizedName }).IsUnique();
            category.HasOne(x => x.Company).WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CompanyProductEntity>(product =>
        {
            product.ToTable("CompanyProducts");
            product.Property(x => x.Name).HasMaxLength(160).IsRequired();
            product.Property(x => x.Description).HasMaxLength(2_000).IsRequired();
            product.Property(x => x.Price).HasPrecision(12, 2);
            product.Property(x => x.ImageUrl).HasMaxLength(2_000);
            product.Property(x => x.WeightOrVolume).HasPrecision(10, 2);
            product.Property(x => x.WeightType).HasMaxLength(16);
            product.HasIndex(x => new { x.CompanyId, x.CategoryId });
            product.HasOne(x => x.Company).WithMany(x => x.Products)
                .HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            product.HasOne(x => x.Category).WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductAddonGroupEntity>(group =>
        {
            group.ToTable("ProductAddonGroups");
            group.Property(x => x.Name).HasMaxLength(120).IsRequired();
            group.HasIndex(x => x.ProductId);
            group.HasOne(x => x.Product).WithMany(x => x.AddonGroups)
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductAddonItemEntity>(item =>
        {
            item.ToTable("ProductAddonItems");
            item.Property(x => x.Name).HasMaxLength(120).IsRequired();
            item.Property(x => x.Price).HasPrecision(12, 2);
            item.HasIndex(x => x.AddonGroupId);
            item.HasOne(x => x.AddonGroup).WithMany(x => x.Items)
                .HasForeignKey(x => x.AddonGroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BranchProductCategoryEntity>(availability =>
        {
            availability.ToTable("BranchProductCategories");
            availability.HasIndex(x => new { x.BranchId, x.CategoryId }).IsUnique();
            availability.HasOne(x => x.Branch).WithMany(x => x.ProductCategories).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Cascade);
            availability.HasOne(x => x.Category).WithMany(x => x.Branches).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BranchProductEntity>(availability =>
        {
            availability.ToTable("BranchProducts");
            availability.HasIndex(x => new { x.BranchId, x.ProductId }).IsUnique();
            availability.HasOne(x => x.Branch).WithMany(x => x.Products).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Cascade);
            availability.HasOne(x => x.Product).WithMany(x => x.Branches).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
