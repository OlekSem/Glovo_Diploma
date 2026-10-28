using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using GlovoBackEnd.Data;
using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Mappers;
using GlovoBackEnd.Models.Dtos.Common;
using GlovoBackEnd.Models.Dtos.Partner;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Controllers.Partner;

[ApiController]
[Route("api/partner/auth")]
public class PartnerAuthController(
    AppDbContext dbContext,
    IPasswordHasher<PartnerEntity> passwordHasher,
    IJwtTokenService jwtTokenService,
    IEmailService emailService,
    IConfiguration configuration) : ControllerBase
{
    private const string AccessCookie = "partner_access_token";
    private const string RefreshCookie = "partner_refresh_token";
    private const string RefreshCookiePath = "/api/partner/auth";
    private readonly string _frontendUrl = configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterPartnerDto dto)
    {
        var normalizedEmail = NormalizeEmail(dto.Email);
        var normalizedPhone = NormalizePhone(dto.Phone);

        if (await dbContext.Partners.AnyAsync(x => !x.IsDeleted && x.NormalizedEmail == normalizedEmail))
            return BadRequest(new { message = "A partner with this email already exists." });

        if (await dbContext.Partners.AnyAsync(x => !x.IsDeleted && x.Phone == normalizedPhone))
            return BadRequest(new { message = "A partner with this phone number already exists." });

        var ownerRole = await dbContext.PartnerRoles
            .SingleAsync(x => x.Name == PartnerRoleNames.CompanyOwner);
        var partner = new PartnerEntity
        {
            Id = Guid.NewGuid(),
            Email = dto.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Phone = normalizedPhone,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            DateCreated = DateTime.UtcNow,
            EmailConfirmed = false
        };
        partner.PasswordHash = passwordHasher.HashPassword(partner, dto.Password);

        var code = SetConfirmationCode(partner);
        partner.PartnerRoles.Add(new PartnerUserRoleEntity { PartnerId = partner.Id, RoleId = ownerRole.Id });
        dbContext.Partners.Add(partner);
        await dbContext.SaveChangesAsync();

        await SendConfirmationCodeAsync(partner.Email, code);

        return Ok(new
        {
            message = "Registration successful. Enter the confirmation code sent to your email.",
            userId = partner.Id
        });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        if (!Guid.TryParse(dto.UserId, out var partnerId))
            return BadRequest(new { message = "Invalid email confirmation request." });

        var partner = await dbContext.Partners.SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == partnerId);
        if (partner == null)
            return BadRequest(new { message = "Invalid email confirmation request." });

        if (partner.EmailConfirmed)
            return BadRequest(new { message = "Email is already confirmed. You can log in." });

        if (partner.ConfirmationCodeExpiresAt < DateTime.UtcNow ||
            string.IsNullOrWhiteSpace(partner.ConfirmationCodeHash) ||
            passwordHasher.VerifyHashedPassword(partner, partner.ConfirmationCodeHash, dto.Code) == PasswordVerificationResult.Failed)
            return BadRequest(new { message = "Invalid or expired confirmation code." });

        partner.EmailConfirmed = true;
        partner.ConfirmationCodeHash = null;
        partner.ConfirmationCodeExpiresAt = null;
        await dbContext.SaveChangesAsync();

        return Ok(new { message = "Thank you for confirming your email. You can now log in." });
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailDto dto)
    {
        var partner = await dbContext.Partners.SingleOrDefaultAsync(x =>
            !x.IsDeleted && x.NormalizedEmail == NormalizeEmail(dto.Email));

        if (partner == null || partner.EmailConfirmed)
            return Ok(new { message = "If your account exists and is unconfirmed, a new confirmation code has been sent." });

        var code = SetConfirmationCode(partner);
        await dbContext.SaveChangesAsync();
        await SendConfirmationCodeAsync(partner.Email, code);

        return Ok(new
        {
            message = "A new confirmation code has been sent.",
            userId = partner.Id
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginPartnerDto dto)
    {
        var partner = await FindPartnerWithRolesAsync(dto.Email);
        if (partner == null || passwordHasher.VerifyHashedPassword(partner, partner.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        if (!partner.EmailConfirmed)
            return Unauthorized(new { message = "Please confirm your email address before logging in." });

        var auth = await jwtTokenService.CreatePartnerAuthResponse(partner);
        SetAuthCookies(auth);
        return Ok(new AuthPartnerResponseDto(partner.ToPartnerResponseDto()));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var partner = await dbContext.Partners.SingleOrDefaultAsync(x =>
            !x.IsDeleted && x.NormalizedEmail == NormalizeEmail(dto.Email));

        if (partner == null || !partner.EmailConfirmed)
            return Ok(new { message = "If your email is registered and confirmed, a password reset link has been sent." });

        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        partner.PasswordResetTokenHash = passwordHasher.HashPassword(partner, token);
        partner.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
        await dbContext.SaveChangesAsync();

        var resetLink = $"{_frontendUrl}/reset-password?email={Uri.EscapeDataString(partner.Email)}&token={Uri.EscapeDataString(token)}";
        try
        {
            await emailService.SendEmailAsync(
                partner.Email,
                "Reset Your Password",
                $"Reset your password by clicking <a href='{resetLink}'>here</a>.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] Failed to send password reset email: {ex.Message}");
        }

        return Ok(new { message = "If your email is registered and confirmed, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var partner = await dbContext.Partners.SingleOrDefaultAsync(x =>
            !x.IsDeleted && x.NormalizedEmail == NormalizeEmail(dto.Email));

        if (partner == null || partner.PasswordResetTokenExpiresAt < DateTime.UtcNow ||
            string.IsNullOrWhiteSpace(partner.PasswordResetTokenHash) ||
            passwordHasher.VerifyHashedPassword(partner, partner.PasswordResetTokenHash, dto.Token) == PasswordVerificationResult.Failed)
            return BadRequest(new { message = "Invalid or expired password reset request." });

        partner.PasswordHash = passwordHasher.HashPassword(partner, dto.NewPassword);
        partner.PasswordResetTokenHash = null;
        partner.PasswordResetTokenExpiresAt = null;
        partner.RefreshToken = null;
        partner.RefreshTokenExpiry = null;
        await dbContext.SaveChangesAsync();

        return Ok(new { message = "Password has been successfully reset. You can now log in." });
    }

    [Authorize(Roles = PartnerRoleNames.AnyPartnerRole)]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var partner = await GetCurrentPartnerAsync();
        if (partner == null)
            return NotFound(new { message = "Partner account not found." });

        if (passwordHasher.VerifyHashedPassword(partner, partner.PasswordHash, dto.CurrentPassword) == PasswordVerificationResult.Failed)
            return BadRequest(new { message = "Current password is incorrect." });

        partner.PasswordHash = passwordHasher.HashPassword(partner, dto.NewPassword);
        partner.RefreshToken = null;
        partner.RefreshTokenExpiry = null;
        await dbContext.SaveChangesAsync();
        DeleteAuthCookies();

        return Ok(new { message = "Password has been successfully changed. You have been logged out." });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshCookie];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token is missing." });

        try
        {
            var auth = await jwtTokenService.RefreshPartnerTokenAsync(refreshToken);
            SetAuthCookies(auth);
            return Ok(new { message = "Token refreshed." });
        }
        catch (UnauthorizedAccessException ex)
        {
            DeleteAuthCookies();
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize(Roles = PartnerRoleNames.AnyPartnerRole)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var partner = await GetCurrentPartnerAsync();
        return partner == null
            ? NotFound(new { message = "Partner account not found." })
            : Ok(partner.ToPartnerResponseDto());
    }

    [Authorize(Roles = PartnerRoleNames.AnyPartnerRole)]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePartnerProfileDto dto)
    {
        var partner = await GetCurrentPartnerAsync();
        if (partner == null)
            return NotFound(new { message = "Partner account not found." });

        var phone = NormalizePhone(dto.Phone);
        var phoneIsInUse = await dbContext.Partners.AnyAsync(x =>
            !x.IsDeleted && x.Id != partner.Id && x.Phone == phone);
        if (phoneIsInUse)
            return BadRequest(new { message = "A partner with this phone number already exists." });

        partner.FirstName = dto.FirstName.Trim();
        partner.LastName = dto.LastName.Trim();
        partner.Phone = phone;
        await dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Partner profile updated successfully.",
            partner = partner.ToPartnerResponseDto()
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshCookie];
        if (!string.IsNullOrEmpty(refreshToken))
            await jwtTokenService.RevokePartnerRefreshTokenAsync(refreshToken);

        DeleteAuthCookies();
        return Ok(new { message = "Logged out successfully." });
    }

    private async Task<PartnerEntity?> FindPartnerWithRolesAsync(string email) =>
        await dbContext.Partners
            .Include(x => x.PartnerRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => !x.IsDeleted && x.NormalizedEmail == NormalizeEmail(email));

    private async Task<PartnerEntity?> GetCurrentPartnerAsync()
    {
        var partnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(partnerId, out var id)
            ? await dbContext.Partners
                .Include(x => x.PartnerRoles)
                .ThenInclude(x => x.Role)
                .SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == id)
            : null;
    }

    private string SetConfirmationCode(PartnerEntity partner)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString();
        partner.ConfirmationCodeHash = passwordHasher.HashPassword(partner, code);
        partner.ConfirmationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
        return code;
    }

    private async Task SendConfirmationCodeAsync(string email, string code)
    {
        try
        {
            await emailService.SendEmailAsync(
                email,
                "Confirm Your Glovo Partner Account",
                $"Please confirm your account with this code: <strong>{code}</strong>.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] Failed to send confirmation email: {ex.Message}");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
    private static string NormalizePhone(string phone) => phone.Trim().Replace(" ", string.Empty).Replace("-", string.Empty);

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
            Path = RefreshCookiePath
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
