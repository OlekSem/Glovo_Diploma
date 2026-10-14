namespace GlovoBackEnd.Models.Dtos.Partner;

public record RegisterPartnerDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public record LoginPartnerDto(
    string Email,
    string Password
);

public record PartnerResponseDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? Image,
    DateTime DateCreated
);

public record AuthTokenResponseDto(
    PartnerResponseDto Partner
);