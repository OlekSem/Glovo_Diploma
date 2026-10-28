namespace GlovoBackEnd.Models.Dtos.Company;

using System.ComponentModel.DataAnnotations;

public sealed record CreateCompanyDto(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, StringLength(1_000, MinimumLength = 10)] string Description
);

public sealed record CompanyResponseDto(
    Guid Id,
    string Name,
    string Description,
    Guid OwnerId,
    DateTime DateCreated
);
