namespace GlovoBackEnd.Models.Dtos.Partner;

using System.ComponentModel.DataAnnotations;

public sealed record RegisterPartnerDto(
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, StringLength(128, MinimumLength = 6)] string Password,
    [property: Required, StringLength(50, MinimumLength = 1), RegularExpression(@"^[\p{L}][\p{L}\p{M}' -]*$")] string FirstName,
    [property: Required, StringLength(50, MinimumLength = 1), RegularExpression(@"^[\p{L}][\p{L}\p{M}' -]*$")] string LastName
);

public sealed record LoginPartnerDto(
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, StringLength(128, MinimumLength = 6)] string Password
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
