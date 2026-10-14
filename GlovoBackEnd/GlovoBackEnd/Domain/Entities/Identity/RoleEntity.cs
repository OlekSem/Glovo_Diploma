namespace GlovoBackEnd.Domain.Entities.Identity;

using Microsoft.AspNetCore.Identity;

public class RoleEntity : IdentityRole<Guid>
{
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}