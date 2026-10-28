namespace GlovoBackEnd.Domain.Entities.Partner;

public class PartnerUserRoleEntity
{
    public Guid PartnerId { get; set; }
    public int RoleId { get; set; }
    public PartnerEntity Partner { get; set; } = null!;
    public PartnerRoleEntity Role { get; set; } = null!;
}
