using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace GlovoBackEnd.Controllers.Courier;

using System.Security.Claims;

using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Mappers;
using GlovoBackEnd.Models.Dtos.Courier;
using GlovoBackEnd.Models.Dtos.Common;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;



[ApiController]
[Route("api/courier/auth")]
public class CourierAuthController(
    UserManager<UserEntity> userManager,
    RoleManager<RoleEntity> roleManager,
    IJwtTokenService jwtTokenService,
    IEmailService emailService,
    IConfiguration configuration) : ControllerBase
{
    private const string CourierRole = "Courier";
    private const string AccessCookie = "courier_access_token";
    private const string RefreshCookie = "courier_refresh_token";
    private const string RefreshCookiePath = "/api/courier/auth";
    private readonly string _frontendUrl = configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCourierDto dto)
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

        if (!await roleManager.RoleExistsAsync(CourierRole))
            await roleManager.CreateAsync(new RoleEntity { Name = CourierRole });

        var roleResult = await userManager.AddToRoleAsync(user, CourierRole);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user); // Rollback if the role was not added to the user
            return BadRequest(roleResult.Errors);
        }
        
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        // Dynamically grabs from appsettings.Development.json (or falls back to localhost)

        var confirmationLink = $"{_frontendUrl}/courier/confirm-email?userId={user.Id}&token={code}";
        
        // send the email via service
        //todo change the subject and possibly add a beautiful html message
        try
        {
            await emailService.SendEmailAsync(
                user.Email,
                "Confirm Your Glovo Courier Account",
                $"Please confirm your account by clicking <a href='{confirmationLink}'>here</a>.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] Failed to send confirmation email: {ex.Message}");
        }
        
        //todo rm this when deploying
        // REMOVE THESE BEFORE DEPLOYING
        Console.WriteLine($"\n==================================================");
        Console.WriteLine($"[SWAGGER DEV] Confirmation Token for {user.Email}:");
        Console.WriteLine($"UserId: {user.Id}");
        Console.WriteLine($"Token: {code}");
        Console.WriteLine($"==================================================\n");

        return Ok(new
        {
            message = "Registration successful. Please check your email (or backend console) to confirm your account.",
            devUserId = user.Id, // REMOVE THESE BEFORE DEPLOYING
            devToken = code, // REMOVE THESE BEFORE DEPLOYING
        });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        var user = await userManager.FindByIdAsync(dto.UserId);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid email confirmation request." });
        }

        if (user.EmailConfirmed)
        {
            return BadRequest(new { message = "Email is already confirmed. You can log in." });
        }

        try
        {
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Invalid or expired confirmation token.", errors = result.Errors });
            }

            return Ok(new { message = "Thank you for confirming your email. You can now log in." });
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Invalid email confirmation request." });
        }
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null || user.EmailConfirmed)
        {
            // not revealing if the email exists
            return Ok(new { message = "If your account exists and is unconfirmed, a new confirmation token has been sent." });
        }
        
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var confirmationLink = $"{_frontendUrl}/courier/confirm-email?userId={user.Id}&token={code}";
        
        // resend the email via service
        //todo change the subject and possibly add a beautiful html message
        try
        {
            await emailService.SendEmailAsync(
                user.Email,
                "Resend: Confirm Your Glovo Courier Account",
                $"Please confirm your account by clicking <a href='{confirmationLink}'>here</a>.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] Failed to resend confirmation email: {ex.Message}");
        }
        
        //todo rm this when deploying
        // REMOVE THESE BEFORE DEPLOYING
        Console.WriteLine($"\n==================================================");
        Console.WriteLine($"[SWAGGER DEV] Resent Confirmation Token for {user.Email}:");
        Console.WriteLine($"UserId: {user.Id}");
        Console.WriteLine($"Token: {code}");
        Console.WriteLine($"==================================================\n");

        return Ok(new
        {
            message = "If your account exists and is unconfirmed, a new confirmation token has been sent.",
            devUserId = user.Id, // REMOVE THESE BEFORE DEPLOYING
            devToken = code, // REMOVE THESE BEFORE DEPLOYING
        });
}

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCourierDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        if (!await userManager.IsEmailConfirmedAsync(user))
            return Unauthorized(new { message = "Please confirm your email address before logging in." });
        
        if (!await userManager.IsInRoleAsync(user, CourierRole))
            return Forbid();

        var auth = await jwtTokenService.CreateAuthResponse(user);
        SetAuthCookies(auth);

        return Ok(new AuthCourierResponseDto(user.ToCourierResponseDto()));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await userManager.IsEmailConfirmedAsync(user))
        {
            return Ok(new { message = "If your email is registered and confirmed, a password reset link has been sent." });
        }
        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var resetLink = $"{_frontendUrl}/courier/reset-password?email={user.Email}&token={code}";
        
        try
        {
            //todo change the subject and possibly add a beautiful html message
            await emailService.SendEmailAsync(
                user.Email,
                "Reset Your Password",
                $"Reset your password by clicking <a href='{resetLink}'>here</a>.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] Failed to send password reset email: {ex.Message}");
        }

        //todo rm this when deploying
        // REMOVE THESE BEFORE DEPLOYING
        Console.WriteLine($"\n==================================================");
        Console.WriteLine($"[SWAGGER DEV] Reset Token for {user.Email}:");
        Console.WriteLine($"Email: {user.Email}");
        Console.WriteLine($"Token: {code}");
        Console.WriteLine($"==================================================\n");

        return Ok(new
        {
            message = "If your email is registered and confirmed, a password reset link has been sent.",
            devEmail = user.Email, // REMOVE THESE BEFORE DEPLOYING
            devToken = code // REMOVE THESE BEFORE DEPLOYING
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid password reset request." });
        try
        {
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to reset password.", errors = result.Errors });
            }
            
            // 🚀 GLOBAL LOGOUT: Clear refresh token so ALL active sessions on all devices are killed
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await userManager.UpdateAsync(user);
            return Ok(new { message = "Password has been successfully reset. You can now log in." });
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Invalid token format." });
        }
    }

    [HttpPost("change-password")]
    [Authorize(Roles = CourierRole)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound(new { message = "Courier user not found." });
        }
        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Failed to change password.", errors = result.Errors });
        }
        
        // 1. Kill the refresh token in the database
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await userManager.UpdateAsync(user);

        // 2. Delete the auth cookies from the browser immediately
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

    [Authorize(Roles = CourierRole)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return NotFound(new { message = "Courier user not found." });

        return Ok(user.ToCourierResponseDto());
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