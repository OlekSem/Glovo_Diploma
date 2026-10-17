namespace GlovoBackEnd.Models.Dtos.Partner;

public sealed record RegisterPartnerDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public sealed record LoginPartnerDto(
    string Email,
    string Password
);

public sealed record PartnerResponseDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? Image,
    DateTime DateCreated
);

public sealed record AuthTokenResponseDto(PartnerResponseDto Partner);