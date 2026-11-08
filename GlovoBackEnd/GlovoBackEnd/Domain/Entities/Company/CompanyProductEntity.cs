namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;

/// <summary>A company-level product. Branch-specific availability is intentionally not modelled yet.</summary>
public class CompanyProductEntity : BaseEntity<Guid>
{
    public Guid CompanyId { get; set; }
    public CompanyEntity Company { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public CompanyProductCategoryEntity Category { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int? Calories { get; set; }
    public decimal? WeightOrVolume { get; set; }
    public string? WeightType { get; set; }
    public ICollection<ProductAddonGroupEntity> AddonGroups { get; set; } = new List<ProductAddonGroupEntity>();
    public ICollection<BranchProductEntity> Branches { get; set; } = new List<BranchProductEntity>();
}
