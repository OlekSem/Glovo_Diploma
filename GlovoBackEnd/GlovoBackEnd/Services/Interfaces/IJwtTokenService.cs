using System.Security.Claims;
using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Models.Dtos.Client;
using GlovoBackEnd.Models.Dtos.Common;

namespace GlovoBackEnd.Services.Interfaces;

public interface IJwtTokenService
{
    Task<string> GenerateToken(UserEntity user);
    string GenerateRefreshToken();
    Task<AuthTokensDto> CreateAuthResponse(UserEntity user);
    Task<AuthTokensDto> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task<AuthTokensDto> CreatePartnerAuthResponse(PartnerEntity partner);
    Task<AuthTokensDto> RefreshPartnerTokenAsync(string refreshToken);
    Task RevokePartnerRefreshTokenAsync(string refreshToken);
}
