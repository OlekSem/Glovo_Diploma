namespace GlovoBackEnd.Models.Dtos.Partner;

using System.ComponentModel.DataAnnotations;

public sealed record RegisterPartnerDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128, MinimumLength = 6)] string Password,
    [Required, StringLength(50, MinimumLength = 1), RegularExpression(@"^[\p{L}][\p{L}\p{M}' -]*$")] string FirstName,
    [Required, StringLength(50, MinimumLength = 1), RegularExpression(@"^[\p{L}][\p{L}\p{M}' -]*$")] string LastName
);

public sealed record LoginPartnerDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128, MinimumLength = 6)] string Password
);

public sealed record PartnerResponseDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? Image,
    DateTime DateCreated
);

public sealed record AuthPartnerResponseDto(PartnerResponseDto Partner);
