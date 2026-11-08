namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;

/// <summary>Physical branch of a partner company where orders can be fulfilled.</summary>
public class CompanyBranchEntity : BaseEntity<Guid>
{
    public Guid CompanyId { get; set; }
    public CompanyEntity Company { get; set; } = null!;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Locality { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public ICollection<BranchProductCategoryEntity> ProductCategories { get; set; } = new List<BranchProductCategoryEntity>();
    public ICollection<BranchProductEntity> Products { get; set; } = new List<BranchProductEntity>();
}
