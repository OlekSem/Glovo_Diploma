namespace GlovoBackEnd.Domain.Entities.Partner;

using GlovoBackEnd.Domain.Entities;
using GlovoBackEnd.Domain.Entities.Company;

/// <summary>
/// A business partner account. This is intentionally separate from the existing
/// Identity user account used by customers and couriers.
/// </summary>
public class PartnerEntity : BaseEntity<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string? ConfirmationCodeHash { get; set; }
    public DateTime? ConfirmationCodeExpiresAt { get; set; }
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public ICollection<PartnerUserRoleEntity> PartnerRoles { get; set; } = new List<PartnerUserRoleEntity>();
    public ICollection<CompanyEntity> Companies { get; set; } = new List<CompanyEntity>();
}
