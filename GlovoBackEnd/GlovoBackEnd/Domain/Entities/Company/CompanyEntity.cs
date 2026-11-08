namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;
using GlovoBackEnd.Domain.Enums;
using GlovoBackEnd.Domain.Entities.Partner;

/// <summary>
/// A company owned by a partner. Future outlets, products and company settings
/// belong to this aggregate.
/// </summary>
public class CompanyEntity : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    // Storage references are intentionally optional until image upload is introduced.
    public string? IconUrl { get; set; }
    public string? BannerUrl { get; set; }
    public CompanyStatus Status { get; set; } = CompanyStatus.Pending;
    public string? ModerationComment { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedById { get; set; }
    public Guid OwnerId { get; set; }
    public PartnerEntity Owner { get; set; } = null!;
    public ICollection<CompanyBranchEntity> Branches { get; set; } = new List<CompanyBranchEntity>();
    public ICollection<CompanyProductCategoryEntity> ProductCategories { get; set; } = new List<CompanyProductCategoryEntity>();
    public ICollection<CompanyProductEntity> Products { get; set; } = new List<CompanyProductEntity>();
}
