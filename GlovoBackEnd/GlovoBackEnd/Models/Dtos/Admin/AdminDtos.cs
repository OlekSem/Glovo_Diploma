namespace GlovoBackEnd.Models.Dtos.Admin;

using System.ComponentModel.DataAnnotations;

public sealed record LoginAdminDto(
    [Required, StringLength(254)] string Identifier,
    [Required, StringLength(128, MinimumLength = 6)] string Password
);

public sealed record AdminResponseDto(Guid Id, string Username, string Email, string? FirstName, string? LastName);
