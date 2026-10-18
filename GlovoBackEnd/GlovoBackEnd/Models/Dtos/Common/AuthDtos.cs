namespace GlovoBackEnd.Models.Dtos.Common;

public record ConfirmEmailDto(string UserId, string Token);
public record ForgotPasswordDto(string Email);
public record ResetPasswordDto(string Email, string Token, string NewPassword);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);
public record ResendConfirmationEmailDto(string Email);
