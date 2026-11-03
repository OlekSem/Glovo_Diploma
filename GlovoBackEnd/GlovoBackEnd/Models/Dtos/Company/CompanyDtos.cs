namespace GlovoBackEnd.Models.Dtos.Company;

using System.ComponentModel.DataAnnotations;
using GlovoBackEnd.Domain.Enums;

public sealed record CreateCompanyDto(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, StringLength(1_000, MinimumLength = 10)] string Description
);

public sealed record CompanyResponseDto(
    Guid Id,
    string Name,
    string Description,
    Guid OwnerId,
    DateTime DateCreated,
    CompanyStatus Status,
    string? ModerationComment,
    DateTime? ReviewedAt
);

public sealed record AdminCompanyResponseDto(
    Guid Id,
    string Name,
    string Description,
    CompanyStatus Status,
    string? ModerationComment,
    DateTime DateCreated,
    DateTime? ReviewedAt,
    Guid OwnerId,
    string OwnerName,
    string OwnerEmail,
    string OwnerPhone
);

public sealed record ModerateCompanyDto(
    [StringLength(1_000)] string? Comment
);
