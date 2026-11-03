using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Models.Dtos.Admin;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GlovoBackEnd.Controllers.Admin;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController(
    UserManager<UserEntity> userManager,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    private const string AccessCookie = "admin_access_token";
    private const string RefreshCookie = "admin_refresh_token";

    [HttpPost("login")]
    public async Task<ActionResult<AdminResponseDto>> Login([FromBody] LoginAdminDto dto)
    {
        var identifier = dto.Identifier.Trim();
        var admin = await userManager.FindByEmailAsync(identifier)
            ?? await userManager.FindByNameAsync(identifier);

        if (admin is null || !await userManager.CheckPasswordAsync(admin, dto.Password) ||
            !await userManager.IsInRoleAsync(admin, ApplicationRoleNames.Administrator))
            return Unauthorized(new { message = "Invalid administrator credentials." });

        var tokens = await jwtTokenService.CreateAuthResponse(admin);
        SetCookies(tokens.AccessToken, tokens.RefreshToken);
        return Ok(ToResponse(admin));
    }

    [Authorize(Roles = ApplicationRoleNames.Administrator)]
    [HttpGet("me")]
    public async Task<ActionResult<AdminResponseDto>> GetMe()
    {
        var admin = await userManager.GetUserAsync(User);
        return admin is null
            ? Unauthorized(new { message = "Administrator session is invalid." })
            : Ok(ToResponse(admin));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshCookie];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token is missing." });

        try
        {
            var tokens = await jwtTokenService.RefreshTokenAsync(refreshToken);
            SetCookies(tokens.AccessToken, tokens.RefreshToken);
            return Ok(new { message = "Token refreshed." });
        }
        catch (UnauthorizedAccessException)
        {
            DeleteCookies();
            return Unauthorized(new { message = "Invalid or expired administrator session." });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshCookie];
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await jwtTokenService.RevokeRefreshTokenAsync(refreshToken);
        DeleteCookies();
        return Ok(new { message = "Logged out successfully." });
    }

    private static AdminResponseDto ToResponse(UserEntity user) =>
        new(user.Id, user.UserName ?? user.Email ?? "Administrator", user.Email ?? string.Empty, user.FirstName, user.LastName);

    private void SetCookies(string accessToken, string refreshToken)
    {
        Response.Cookies.Append(AccessCookie, accessToken, new CookieOptions
        {
            HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(30), Path = "/"
        });
        Response.Cookies.Append(RefreshCookie, refreshToken, new CookieOptions
        {
            HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/api/admin/auth"
        });
    }

    private void DeleteCookies()
    {
        Response.Cookies.Delete(AccessCookie, new CookieOptions { Path = "/", Secure = Request.IsHttps, SameSite = SameSiteMode.Lax });
        Response.Cookies.Delete(RefreshCookie, new CookieOptions { Path = "/api/admin/auth", Secure = Request.IsHttps, SameSite = SameSiteMode.Lax });
    }
}
