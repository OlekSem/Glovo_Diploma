namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;

public class CompanyProductCategoryEntity : BaseEntity<Guid>
{
    public Guid CompanyId { get; set; }
    public CompanyEntity Company { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public ICollection<CompanyProductEntity> Products { get; set; } = new List<CompanyProductEntity>();
    public ICollection<BranchProductCategoryEntity> Branches { get; set; } = new List<BranchProductCategoryEntity>();
}
