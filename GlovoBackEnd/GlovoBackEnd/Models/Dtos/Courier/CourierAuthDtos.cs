namespace GlovoBackEnd.Models.Dtos.Courier;


public sealed record RegisterCourierDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public sealed record LoginCourierDto(
    string Email,
    string Password
);
public sealed record CourierResponseDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? Image,
    DateTime DateCreated
);

public sealed record AuthCourierResponseDto(CourierResponseDto Courier);