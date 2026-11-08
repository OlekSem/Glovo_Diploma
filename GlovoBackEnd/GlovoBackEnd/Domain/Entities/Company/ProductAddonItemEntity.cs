namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;

public class ProductAddonItemEntity : BaseEntity<Guid>
{
    public Guid AddonGroupId { get; set; }
    public ProductAddonGroupEntity AddonGroup { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
