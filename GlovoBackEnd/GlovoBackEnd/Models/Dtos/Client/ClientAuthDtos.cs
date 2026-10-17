namespace GlovoBackEnd.Models.Dtos.Client;


public sealed record RegisterClientDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public sealed record LoginClientDto(
    string Email,
    string Password
);
public sealed record ClientResponseDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? Image,
    DateTime DateCreated
);

public sealed record AuthClientResponseDto(ClientResponseDto Client);