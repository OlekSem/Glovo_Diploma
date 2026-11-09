namespace GlovoBackEnd.Models.Dtos.Company;

using System.ComponentModel.DataAnnotations;

public sealed record CreateProductCategoryDto(
    [Required, StringLength(120, MinimumLength = 2)] string Name
);

public sealed record ProductCategoryResponseDto(Guid Id, Guid CompanyId, string Name, int SortOrder, DateTime DateCreated);

public sealed record UpdateProductCategoryDto(
    [Required, StringLength(120, MinimumLength = 2)] string Name
);

public sealed record ReorderProductCategoriesDto(
    [Required, MinLength(1)] IReadOnlyCollection<Guid> CategoryIds
);

public sealed record CreateProductDto(
    [Required, StringLength(160, MinimumLength = 2)] string Name,
    [Required, StringLength(2_000, MinimumLength = 2)] string Description,
    [Range(0, 9_999_999_999.99)] decimal Price,
    [Range(0, 100_000)] int? Calories,
    [Range(0.01, 99_999_999.99)] decimal? WeightOrVolume,
    [StringLength(16)] string? WeightType
);

public sealed record ProductResponseDto(
    Guid Id,
    Guid CompanyId,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    int? Calories,
    decimal? WeightOrVolume,
    string? WeightType,
    DateTime DateCreated
);

public sealed record CreateAddonItemDto(
    [Required, StringLength(120, MinimumLength = 1)] string Name,
    [Range(0, 9_999_999_999.99)] decimal Price
);

public sealed record CreateProductAddonGroupDto(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Range(0, 100)] int MinimumSelection,
    [Range(0, 100)] int MaximumSelection,
    [Required, MinLength(1)] IReadOnlyCollection<CreateAddonItemDto> Items
);

public sealed record ProductAddonItemResponseDto(Guid Id, string Name, decimal Price);

public sealed record ProductAddonGroupResponseDto(
    Guid Id,
    Guid ProductId,
    string Name,
    int MinimumSelection,
    int MaximumSelection,
    IReadOnlyCollection<ProductAddonItemResponseDto> Items,
    DateTime DateCreated
);
public sealed record ClientProductResponseDto(
    Guid Id,
    Guid CompanyId,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    int? Calories,
    decimal? WeightOrVolume,
    string? WeightType,
    List<Guid> AvailableBranchIds
    );