namespace GlovoBackEnd.Models.Dtos.Common;

using System.ComponentModel.DataAnnotations;

public record ConfirmEmailDto(
    [property: Required, RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")] string UserId,
    [property: Required, MinLength(1)] string Token);

public record ForgotPasswordDto([property: Required, EmailAddress, StringLength(254)] string Email);

public record ResetPasswordDto(
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, MinLength(1)] string Token,
    [property: Required, StringLength(128, MinimumLength = 6)] string NewPassword);

public record ChangePasswordDto(
    [property: Required, StringLength(128, MinimumLength = 6)] string CurrentPassword,
    [property: Required, StringLength(128, MinimumLength = 6)] string NewPassword);

public record ResendConfirmationEmailDto([property: Required, EmailAddress, StringLength(254)] string Email);
