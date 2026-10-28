namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;
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
    public Guid OwnerId { get; set; }
    public PartnerEntity Owner { get; set; } = null!;
}
