using System.Security.Claims;

using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Mappers;
using GlovoBackEnd.Models.Dtos.Common;
using GlovoBackEnd.Models.Dtos.Partner;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GlovoBackEnd.Controllers.Partner;

[ApiController]
[Route("api/partner/auth")]
public class PartnerAuthController(
    UserManager<UserEntity> userManager,
    RoleManager<RoleEntity> roleManager,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    private const string PartnerRole = "Partner";
    private const string AccessCookie = "access_token";
    private const string RefreshCookie = "refresh_token";
    private const string RefreshCookiePath = "/api/partner/auth";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterPartnerDto dto)
    {
        if (await userManager.FindByEmailAsync(dto.Email) != null)
            return BadRequest(new { message = "User with this email already exists." });

        var user = new UserEntity
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateCreated = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (!await roleManager.RoleExistsAsync(PartnerRole))
            await roleManager.CreateAsync(new RoleEntity { Name = PartnerRole });

        var roleResult = await userManager.AddToRoleAsync(user, PartnerRole);
        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);

        var auth = await jwtTokenService.CreateAuthResponse(user);
        SetAuthCookies(auth);

        return Ok(new AuthTokenResponseDto(user.ToPartnerResponseDto()));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginPartnerDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        if (!await userManager.IsInRoleAsync(user, PartnerRole))
            return Forbid();

        var auth = await jwtTokenService.CreateAuthResponse(user);
        SetAuthCookies(auth);

        return Ok(new AuthTokenResponseDto(user.ToPartnerResponseDto()));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshCookie];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token is missing." });

        try
        {
            var auth = await jwtTokenService.RefreshTokenAsync(refreshToken);
            SetAuthCookies(auth);
            return Ok(new { message = "Token refreshed." });
        }
        catch (UnauthorizedAccessException ex)
        {
            DeleteAuthCookies();
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize(Roles = PartnerRole)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return NotFound(new { message = "Partner user not found." });

        return Ok(user.ToPartnerResponseDto());
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshCookie];
        if (!string.IsNullOrEmpty(refreshToken))
            await jwtTokenService.RevokeRefreshTokenAsync(refreshToken);

        DeleteAuthCookies();
        return Ok(new { message = "Logged out successfully." });
    }

    private async Task<UserEntity?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        return Guid.TryParse(userId, out var id)
            ? await userManager.FindByIdAsync(id.ToString())
            : null;
    }

    private void SetAuthCookies(AuthTokensDto auth)
    {
        Response.Cookies.Append(AccessCookie, auth.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            Path = "/"
        });

        Response.Cookies.Append(RefreshCookie, auth.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = RefreshCookiePath // refresh-кукі йде лише на ці ендпоінти
        });
    }

    private void DeleteAuthCookies()
    {
        Response.Cookies.Delete(AccessCookie, new CookieOptions
        {
            Path = "/",
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });

        Response.Cookies.Delete(RefreshCookie, new CookieOptions
        {
            Path = RefreshCookiePath,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });
    }
}