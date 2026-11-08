namespace GlovoBackEnd.Models.Dtos.Company;

public sealed record BranchMenuCategoryDto(Guid Id, string Name, bool IsAvailable);
public sealed record BranchMenuProductDto(Guid Id, string Name, string Description, decimal Price, string? ImageUrl, bool IsAvailable);
public sealed record SetAvailabilityDto(bool IsAvailable);
