namespace GlovoBackEnd.Models.Dtos.Common;

using System.ComponentModel.DataAnnotations;

public record ConfirmEmailDto(
    [Required, RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")] string UserId,
    [Required, MinLength(1)] string Token);

public record ForgotPasswordDto([Required, EmailAddress, StringLength(254)] string Email);

public record ResetPasswordDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, MinLength(1)] string Token,
    [Required, StringLength(128, MinimumLength = 6)] string NewPassword);

public record ChangePasswordDto(
    [Required, StringLength(128, MinimumLength = 6)] string CurrentPassword,
    [Required, StringLength(128, MinimumLength = 6)] string NewPassword);

public record ResendConfirmationEmailDto([Required, EmailAddress, StringLength(254)] string Email);
