namespace GlovoBackEnd.Domain.Entities.Partner;

public class PartnerRoleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<PartnerUserRoleEntity> PartnerUsers { get; set; } = new List<PartnerUserRoleEntity>();
}
