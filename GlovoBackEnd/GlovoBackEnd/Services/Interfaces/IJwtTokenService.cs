using System.Security.Claims;
using GlovoBackEnd.Domain.Entities.Identity;

namespace GlovoBackEnd.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(UserEntity user, IEnumerable<string> roles);
}