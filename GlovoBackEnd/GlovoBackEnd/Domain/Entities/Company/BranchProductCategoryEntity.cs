namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;

public class BranchProductCategoryEntity : BaseEntity<Guid>
{
    public Guid BranchId { get; set; }
    public CompanyBranchEntity Branch { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public CompanyProductCategoryEntity Category { get; set; } = null!;
    public bool IsAvailable { get; set; }
}
