namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;

public class ProductAddonGroupEntity : BaseEntity<Guid>
{
    public Guid ProductId { get; set; }
    public CompanyProductEntity Product { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int MinimumSelection { get; set; }
    public int MaximumSelection { get; set; }
    public ICollection<ProductAddonItemEntity> Items { get; set; } = new List<ProductAddonItemEntity>();
}
