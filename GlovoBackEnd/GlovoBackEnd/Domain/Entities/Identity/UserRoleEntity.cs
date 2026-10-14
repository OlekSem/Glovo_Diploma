namespace GlovoBackEnd.Domain.Entities.Identity;

using Microsoft.AspNetCore.Identity;

public class UserRoleEntity : IdentityUserRole<Guid>
{
    public virtual UserEntity User { get; set; } = null!;
    public virtual RoleEntity Role { get; set; } = null!;
}