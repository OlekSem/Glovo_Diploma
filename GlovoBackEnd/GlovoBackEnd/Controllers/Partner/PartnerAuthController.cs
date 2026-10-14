using System.Security.Claims;
using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Mappers;
using GlovoBackEnd.Models.Dtos.Partner;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GlovoBackEnd.Controllers.Partner;

[ApiController]
[Route("api/partner/auth")]
public class PartnerAuthController : ControllerBase
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<RoleEntity> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;

    public PartnerAuthController(
        UserManager<UserEntity> userManager,
        RoleManager<RoleEntity> roleManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterPartnerDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User with this email already exists." });
        }

        var user = new UserEntity
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateCreated = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        if (!await _roleManager.RoleExistsAsync("Partner"))
        {
            await _roleManager.CreateAsync(new RoleEntity { Name = "Partner" });
        }
        await _userManager.AddToRoleAsync(user, "Partner");

        var roles = new[] { "Partner" };
        var token = _jwtTokenService.GenerateToken(user, roles);

        SetJwtCookie(token);

        return Ok(new AuthTokenResponseDto(MapToPartnerResponse(user)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginPartnerDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Partner"))
        {
            return Forbid();
        }

        var token = _jwtTokenService.GenerateToken(user, roles);

        SetJwtCookie(token);

        return Ok(new AuthTokenResponseDto(MapToPartnerResponse(user)));
    }

    [Authorize(Roles = "Partner")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound(new { message = "Partner user not found." });
        }

        return Ok(user.ToPartnerResponseDto());
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token", new CookieOptions
        {
            Path = "/",
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });

        return Ok(new { message = "Logged out successfully." });
    }

    private static PartnerResponseDto MapToPartnerResponse(UserEntity user) =>
        new(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Image,
            user.DateCreated
        );

    private void SetJwtCookie(string token)
    {
        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/"
        });
    }
}