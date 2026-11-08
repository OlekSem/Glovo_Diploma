namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;

public class BranchProductEntity : BaseEntity<Guid>
{
    public Guid BranchId { get; set; }
    public CompanyBranchEntity Branch { get; set; } = null!;
    public Guid ProductId { get; set; }
    public CompanyProductEntity Product { get; set; } = null!;
    public bool IsAvailable { get; set; }
}
