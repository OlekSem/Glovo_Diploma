using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GlovoBackEnd.Domain.Entities.Identity;

using GlovoBackEnd.Models.Dtos.Common;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GlovoBackEnd.Services.Implementations;

public class JwtTokenService(IConfiguration configuration, UserManager<UserEntity> userManager) : IJwtTokenService
{
    public async Task<string> GenerateToken(UserEntity user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("email", user.Email ?? ""),
            new("username", user.UserName ?? ""),
            new("image", user.Image ?? ""),
            new("firstName", user.FirstName ?? ""),
            new("lastName", user.LastName ?? ""),
        };

        foreach (var role in await userManager.GetRolesAsync(user))
            claims.Add(new Claim("roles", role));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string Hash(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public async Task<AuthTokensDto> CreateAuthResponse(UserEntity user)
    {
        var accessToken = await GenerateToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = Hash(refreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException("Failed to save refresh token");

        return new AuthTokensDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken // назовні віддається лише «сирий» токен
        };
    }

    public async Task<AuthTokensDto> RefreshTokenAsync(string refreshToken)
    {
        var hash = Hash(refreshToken);
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.RefreshToken == hash);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        return await CreateAuthResponse(user);
    }
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var hash = Hash(refreshToken);
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.RefreshToken == hash);
        if (user == null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await userManager.UpdateAsync(user);
    }
}