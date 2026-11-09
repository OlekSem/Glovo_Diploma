namespace GlovoBackEnd.Domain.Entities.Company;

using GlovoBackEnd.Domain.Entities;
using NetTopologySuite.Geometries;

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
    /// <summary>
    /// Geographic location of the branch in WGS 84 (SRID 4326).
    /// Stored by PostGIS as a single geography point rather than separate database columns.
    /// </summary>
    public Point? Location { get; set; }
    public ICollection<BranchProductCategoryEntity> ProductCategories { get; set; } = new List<BranchProductCategoryEntity>();
    public ICollection<BranchProductEntity> Products { get; set; } = new List<BranchProductEntity>();
}
