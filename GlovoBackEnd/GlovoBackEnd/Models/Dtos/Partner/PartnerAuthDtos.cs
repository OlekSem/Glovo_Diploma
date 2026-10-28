namespace GlovoBackEnd.Models.Dtos.Partner;

using System.ComponentModel.DataAnnotations;

public sealed record RegisterPartnerDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128, MinimumLength = 6)] string Password,
    [Required, StringLength(50, MinimumLength = 1), RegularExpression(@"^[\p{L}][\p{L}\p{M}' -]*$")] string FirstName,
    [Required, StringLength(50, MinimumLength = 1), RegularExpression(@"^[\p{L}][\p{L}\p{M}' -]*$")] string LastName,
    [Required, StringLength(32, MinimumLength = 7), Phone] string Phone
);

public sealed record LoginPartnerDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128, MinimumLength = 6)] string Password
);

public sealed record PartnerResponseDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    IReadOnlyCollection<string> Roles,
    DateTime DateCreated
);

public sealed record AuthPartnerResponseDto(PartnerResponseDto Partner);

public sealed record UpdatePartnerProfileDto(
    [Required, StringLength(50, MinimumLength = 1), RegularExpression(@"^[\p{L}][\p{L}\p{M}' -]*$")] string FirstName,
    [Required, StringLength(50, MinimumLength = 1), RegularExpression(@"^[\p{L}][\p{L}\p{M}' -]*$")] string LastName,
    [Required, StringLength(32, MinimumLength = 7), Phone] string Phone
);
