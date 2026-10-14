namespace GlovoBackEnd.Domain.Entities.Identity;

using Microsoft.AspNetCore.Identity;

public class UserEntity : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Image { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}