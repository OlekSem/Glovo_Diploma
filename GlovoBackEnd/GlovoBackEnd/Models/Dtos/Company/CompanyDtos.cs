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
    DateTime? ReviewedAt,
    string? Category,
    string? Subcategory,
    string? IconUrl,
    string? BannerUrl
);

public sealed record UpdateCompanyDto(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, StringLength(1_000, MinimumLength = 10)] string Description,
    [StringLength(120)] string? Category,
    [StringLength(120)] string? Subcategory
);

public sealed record CreateCompanyBranchDto(
    [Required, StringLength(32, MinimumLength = 7)] string Phone,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(120, MinimumLength = 2)] string Locality,
    [Required, StringLength(120, MinimumLength = 2)] string Region,
    [Required, StringLength(250, MinimumLength = 5)] string Address,
    [Required, StringLength(12, MinimumLength = 3)] string PostalCode
);

public sealed record CompanyBranchResponseDto(
    Guid Id,
    Guid CompanyId,
    string Phone,
    string Email,
    string Locality,
    string Region,
    string Address,
    string PostalCode,
    DateTime DateCreated
);

public sealed record UpdateCompanyBranchDto(
    [Required, StringLength(32, MinimumLength = 7)] string Phone,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(120, MinimumLength = 2)] string Locality,
    [Required, StringLength(120, MinimumLength = 2)] string Region,
    [Required, StringLength(250, MinimumLength = 5)] string Address,
    [Required, StringLength(12, MinimumLength = 3)] string PostalCode
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
public sealed record ClientCompanyResponseDto(
    Guid Id,
    string Name,
    string Description,
    string? Category,
    string? Subcategory,
    string? IconUrl,
    string? BannerUrl,
    int BranchesCount
);
public sealed record ClientCompanyBranchResponseDto(
    Guid Id,
    Guid CompanyId,
    string Phone,
    string Email,
    string Locality,
    string Region,
    string Address
);


