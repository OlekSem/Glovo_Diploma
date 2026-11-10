using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace GlovoBackEnd.Controllers.Client;

using System.Security.Claims;

using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Mappers;
using GlovoBackEnd.Models.Dtos.Client;
using GlovoBackEnd.Models.Dtos.Common;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;



[ApiController]
[Route("api/client/auth")]
public class ClientAuthController(
    UserManager<UserEntity> userManager,
    RoleManager<RoleEntity> roleManager,
    IJwtTokenService jwtTokenService,
    IEmailService emailService,
    IConfiguration configuration) : ControllerBase
{
    private const string ClientRole = "Client";
    private const string AccessCookie = "Client_access_token";
    private const string RefreshCookie = "Client_refresh_token";
    private const string RefreshCookiePath = "/api/Client/auth";
    private const string ResetPurpose = "ClientResetPassword";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterClientDto dto)
    {
        if (await userManager.FindByEmailAsync(dto.Email) != null)
            return BadRequest(new { message = "User with this email already exists." });

        var user = new UserEntity
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateCreated = DateTime.UtcNow,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (!await roleManager.RoleExistsAsync(ClientRole))
            await roleManager.CreateAsync(new RoleEntity { Name = ClientRole });

        var roleResult = await userManager.AddToRoleAsync(user, ClientRole);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return BadRequest(roleResult.Errors);
        }

        var code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        await SendEmailAsync(user.Email!, "Confirm Your Glovo Client Account",
            $"Please confirm your account with this code: <strong>{code}</strong>.");
        DevLog("Confirmation code", user, code);

        return Ok(new
        {
            message = "Registration successful. Enter the confirmation code sent to your email.",
            // userId = user.Id
        });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmClientEmailDto dto)
    {
        const string invalid = "Invalid or expired confirmation code.";

        var user = await FindClientByEmailAsync(dto.Email);
        if (user == null || user.EmailConfirmed)
            return BadRequest(new { message = invalid });

        var valid = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, dto.Code);
        if (!valid)
            return BadRequest(new { message = invalid });

        user.EmailConfirmed = true;
        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
            return StatusCode(500, new { message = "Could not confirm email." });

        return Ok(new { message = "Email confirmed. You can now log in." });
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailDto dto)
    {
        const string generic = "If your account exists and is unconfirmed, a new confirmation code has been sent.";

        var user = await FindClientByEmailAsync(dto.Email);
        if (user == null || user.EmailConfirmed)
            return Ok(new { message = generic });

        var code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        await SendEmailAsync(user.Email!, "Confirm Your Glovo Client Account",
            $"Please confirm your account with this code: <strong>{code}</strong>.");
        DevLog("Resent confirmation code", user, code);

        return Ok(new { message = generic});
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginClientDto dto)
    {
        var user = await FindClientByEmailAsync(dto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        if (!user.EmailConfirmed)
            return Unauthorized(new { message = "Please confirm your email address before logging in." });

        var auth = await jwtTokenService.CreateAuthResponse(user);

        // Browsers get cookies; React Native reads the tokens from the body.
        if (IsBrowser())
            SetAuthCookies(auth);

        return Ok(new
        {
            message = "Login successful.",
            user = user.ToClientResponseDto(),
            tokens = new { accessToken = auth.AccessToken, refreshToken = auth.RefreshToken }
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        const string generic = "If your email is registered and confirmed, a password reset code has been sent.";

        var user = await FindClientByEmailAsync(dto.Email);
        if (user == null || !user.EmailConfirmed)
            return Ok(new { message = generic });

        var code = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, ResetPurpose);
        await SendEmailAsync(user.Email!, "Reset Your Password",
            $"Your password reset code is: <strong>{code}</strong>.");
        DevLog("Password reset code", user, code);

        return Ok(new { message = generic });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        const string invalid = "Invalid or expired password reset request.";

        var user = await FindClientByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { message = invalid });

        // dto.Token carries the 6-digit code.
        var valid = await userManager.VerifyUserTokenAsync(
            user, TokenOptions.DefaultEmailProvider, ResetPurpose, dto.Token);
        if (!valid)
            return BadRequest(new { message = invalid });

        // Code is valid: use an internal Identity token so password rules still apply.
        var identityToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, identityToken, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to reset password.", errors = result.Errors });

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
            return StatusCode(500, new { message = "Password changed, but session invalidation failed." });

        return Ok(new { message = "Password successfully reset. Please log in again." });
    }

    [HttpPost("change-password")]
    [Authorize(Roles = ClientRole)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return NotFound(new { message = "Client user not found." });

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to change password.", errors = result.Errors });

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
            return StatusCode(500, new { message = "Password changed, but session invalidation failed." });

        DeleteAuthCookies();
        return Ok(new { message = "Password successfully changed. Please log in again." });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto? dto)
    {
        var refreshToken = !string.IsNullOrWhiteSpace(dto?.RefreshToken)
            ? dto!.RefreshToken
            : Request.Cookies[RefreshCookie];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token is required." });

        try
        {
            var auth = await jwtTokenService.RefreshTokenAsync(refreshToken);
            if (IsBrowser())
                SetAuthCookies(auth);

            return Ok(new
            {
                message = "Token refreshed successfully.",
                tokens = new { accessToken = auth.AccessToken, refreshToken = auth.RefreshToken }
            });
        }
        catch (UnauthorizedAccessException)
        {
            DeleteAuthCookies();
            return Unauthorized(new { message = "Invalid or expired refresh token. Please log in again." });
        }
    }

    [Authorize(Roles = ClientRole)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await GetCurrentUserAsync();
        return user == null
            ? NotFound(new { message = "Client user not found." })
            : Ok(user.ToClientResponseDto());
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto? dto)
    {
        var refreshToken = !string.IsNullOrWhiteSpace(dto?.RefreshToken)
            ? dto!.RefreshToken
            : Request.Cookies[RefreshCookie];

        if (!string.IsNullOrWhiteSpace(refreshToken))
            await jwtTokenService.RevokeRefreshTokenAsync(refreshToken);

        DeleteAuthCookies();
        return Ok(new { message = "Logged out successfully." });
    }

    // ---------- helpers ----------

    private bool IsBrowser() => Request.Headers.UserAgent.ToString().Contains("Mozilla");

    private async Task<UserEntity?> FindClientByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user != null && await userManager.IsInRoleAsync(user, ClientRole) ? user : null;
    }

    private async Task<UserEntity?> FindClientByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user != null && await userManager.IsInRoleAsync(user, ClientRole) ? user : null;
    }

    private async Task<UserEntity?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(userId, out var id)
            ? await userManager.FindByIdAsync(id.ToString())
            : null;
    }

    private async Task SendEmailAsync(string email, string subject, string html)
    {
        try
        {
            await emailService.SendEmailAsync(email, subject, html);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] Failed to send '{subject}': {ex.Message}");
        }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private static void DevLog(string label, UserEntity user, string code) =>
        Console.WriteLine($"[DEV] {label} for {user.Email} (UserId {user.Id}): {code}");

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
            Path = "/", Secure = Request.IsHttps, SameSite = SameSiteMode.Lax
        });
        Response.Cookies.Delete(RefreshCookie, new CookieOptions
        {
            Path = RefreshCookiePath, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax
        });
    }
}