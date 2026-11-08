using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GlovoBackEnd.Data;
using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Domain.Enums;
using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Models.Dtos.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Controllers.Partner;

[ApiController]
[Authorize(Roles = PartnerRoleNames.CompanyOwner)]
[Route("api/partner/companies/{companyId:guid}/catalog")]
public class PartnerCompanyCatalogController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyCollection<ProductCategoryResponseDto>>> GetCategories(Guid companyId)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        if (!await IsOwnedActiveCompany(companyId, partnerId)) return NotFound(new { message = "Company not found or is not active." });
        return Ok(await dbContext.CompanyProductCategories.Where(x => x.CompanyId == companyId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => ToResponse(x)).ToListAsync());
    }

    [HttpPost("categories")]
    public async Task<ActionResult<ProductCategoryResponseDto>> CreateCategory(Guid companyId, [FromBody] CreateProductCategoryDto dto)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        if (!await IsOwnedActiveCompany(companyId, partnerId)) return NotFound(new { message = "Company not found or is not active." });
        var normalizedName = Normalize(dto.Name);
        if (await dbContext.CompanyProductCategories.AnyAsync(x => x.CompanyId == companyId && x.NormalizedName == normalizedName))
            return BadRequest(new { message = "A category with this name already exists." });

        var sortOrder = await dbContext.CompanyProductCategories.Where(x => x.CompanyId == companyId).Select(x => (int?)x.SortOrder).MaxAsync() ?? -1;
        var category = new CompanyProductCategoryEntity { Id = Guid.NewGuid(), CompanyId = companyId, Name = dto.Name.Trim(), NormalizedName = normalizedName, SortOrder = sortOrder + 1, DateCreated = DateTime.UtcNow };
        dbContext.CompanyProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategories), new { companyId }, ToResponse(category));
    }

    [HttpPut("categories/{categoryId:guid}")]
    public async Task<ActionResult<ProductCategoryResponseDto>> UpdateCategory(Guid companyId, Guid categoryId, [FromBody] UpdateProductCategoryDto dto)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        var category = await FindCategory(companyId, categoryId, partnerId);
        if (category is null) return NotFound(new { message = "Category not found." });
        var normalizedName = Normalize(dto.Name);
        if (await dbContext.CompanyProductCategories.AnyAsync(x => x.CompanyId == companyId && x.Id != categoryId && x.NormalizedName == normalizedName))
            return BadRequest(new { message = "A category with this name already exists." });
        category.Name = dto.Name.Trim();
        category.NormalizedName = normalizedName;
        await dbContext.SaveChangesAsync();
        return Ok(ToResponse(category));
    }

    [HttpPut("categories/order")]
    public async Task<IActionResult> ReorderCategories(Guid companyId, [FromBody] ReorderProductCategoriesDto dto)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        if (!await IsOwnedActiveCompany(companyId, partnerId)) return NotFound(new { message = "Company not found or is not active." });
        var ids = dto.CategoryIds.ToList();
        if (ids.Distinct().Count() != ids.Count) return BadRequest(new { message = "Category IDs must be unique." });
        var categories = await dbContext.CompanyProductCategories.Where(x => x.CompanyId == companyId && !x.IsDeleted).ToListAsync();
        if (categories.Count != ids.Count || categories.Any(x => !ids.Contains(x.Id))) return BadRequest(new { message = "Provide every active category exactly once." });
        foreach (var category in categories) category.SortOrder = ids.IndexOf(category.Id);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("categories/{categoryId:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid companyId, Guid categoryId)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        var category = await FindCategory(companyId, categoryId, partnerId);
        if (category is null) return NotFound(new { message = "Category not found." });
        dbContext.CompanyProductCategories.Remove(category);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("categories/{categoryId:guid}/products")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponseDto>>> GetProducts(Guid companyId, Guid categoryId)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        if (await FindCategory(companyId, categoryId, partnerId) is null) return NotFound(new { message = "Category not found." });
        return Ok(await dbContext.CompanyProducts.Where(x => x.CompanyId == companyId && x.CategoryId == categoryId && !x.IsDeleted)
            .OrderBy(x => x.Name).Select(x => ToResponse(x)).ToListAsync());
    }

    [HttpPost("categories/{categoryId:guid}/products")]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(Guid companyId, Guid categoryId, [FromBody] CreateProductDto dto)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        if (await FindCategory(companyId, categoryId, partnerId) is null) return NotFound(new { message = "Category not found." });
        var weightType = NormalizeWeightType(dto.WeightType);
        if (!string.IsNullOrWhiteSpace(dto.WeightType) && weightType is null)
            return BadRequest(new { message = "Weight type must be g or ml." });
        if ((weightType is null && dto.WeightOrVolume is not null) || (weightType is not null && dto.WeightOrVolume is null))
            return BadRequest(new { message = "Specify both weight/volume and its type, or leave both empty." });

        var product = new CompanyProductEntity { Id = Guid.NewGuid(), CompanyId = companyId, CategoryId = categoryId, Name = dto.Name.Trim(), Description = dto.Description.Trim(), Price = dto.Price, Calories = dto.Calories, WeightOrVolume = dto.WeightOrVolume, WeightType = weightType, DateCreated = DateTime.UtcNow };
        dbContext.CompanyProducts.Add(product);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProducts), new { companyId, categoryId }, ToResponse(product));
    }

    [HttpDelete("products/{productId:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid companyId, Guid productId)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        var product = await FindProduct(companyId, productId, partnerId);
        if (product is null) return NotFound(new { message = "Product not found." });
        dbContext.CompanyProducts.Remove(product);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("products/{productId:guid}/addons")]
    public async Task<ActionResult<IReadOnlyCollection<ProductAddonGroupResponseDto>>> GetAddons(Guid companyId, Guid productId)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        if (await FindProduct(companyId, productId, partnerId) is null) return NotFound(new { message = "Product not found." });
        var groups = await dbContext.ProductAddonGroups.Where(x => x.ProductId == productId && !x.IsDeleted).Include(x => x.Items.Where(item => !item.IsDeleted)).OrderBy(x => x.Name).ToListAsync();
        return Ok(groups.Select(ToResponse).ToList());
    }

    [HttpPost("products/{productId:guid}/addons")]
    public async Task<ActionResult<ProductAddonGroupResponseDto>> CreateAddonGroup(Guid companyId, Guid productId, [FromBody] CreateProductAddonGroupDto dto)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        if (await FindProduct(companyId, productId, partnerId) is null) return NotFound(new { message = "Product not found." });
        if (dto.MinimumSelection > dto.MaximumSelection || dto.MaximumSelection > dto.Items.Count)
            return BadRequest(new { message = "Selection limits must fit the number of add-ons." });

        var group = new ProductAddonGroupEntity { Id = Guid.NewGuid(), ProductId = productId, Name = dto.Name.Trim(), MinimumSelection = dto.MinimumSelection, MaximumSelection = dto.MaximumSelection, DateCreated = DateTime.UtcNow, Items = dto.Items.Select(item => new ProductAddonItemEntity { Id = Guid.NewGuid(), Name = item.Name.Trim(), Price = item.Price, DateCreated = DateTime.UtcNow }).ToList() };
        dbContext.ProductAddonGroups.Add(group);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAddons), new { companyId, productId }, ToResponse(group));
    }

    [HttpDelete("addons/{addonGroupId:guid}")]
    public async Task<IActionResult> DeleteAddonGroup(Guid companyId, Guid addonGroupId)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        var group = await dbContext.ProductAddonGroups.Include(x => x.Product).SingleOrDefaultAsync(x => x.Id == addonGroupId && !x.IsDeleted && !x.Product.IsDeleted && x.Product.CompanyId == companyId && x.Product.Company.OwnerId == partnerId && x.Product.Company.Status == CompanyStatus.Approved && !x.Product.Company.IsDeleted);
        if (group is null) return NotFound(new { message = "Add-on group not found." });
        dbContext.ProductAddonGroups.Remove(group);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("addons/{addonGroupId:guid}")]
    public async Task<ActionResult<ProductAddonGroupResponseDto>> UpdateAddonGroup(Guid companyId, Guid addonGroupId, [FromBody] CreateProductAddonGroupDto dto)
    {
        if (!TryGetPartnerId(out var partnerId)) return Unauthorized(new { message = "Invalid partner session." });
        var group = await dbContext.ProductAddonGroups.Include(x => x.Items).Include(x => x.Product).ThenInclude(x => x.Company).SingleOrDefaultAsync(x => x.Id == addonGroupId && !x.IsDeleted && !x.Product.IsDeleted && x.Product.CompanyId == companyId && x.Product.Company.OwnerId == partnerId && x.Product.Company.Status == CompanyStatus.Approved && !x.Product.Company.IsDeleted);
        if (group is null) return NotFound(new { message = "Add-on group not found." });
        if (dto.MinimumSelection > dto.MaximumSelection || dto.MaximumSelection > dto.Items.Count) return BadRequest(new { message = "Selection limits must fit the number of add-ons." });
        dbContext.ProductAddonItems.RemoveRange(group.Items);
        group.Name = dto.Name.Trim();
        group.MinimumSelection = dto.MinimumSelection;
        group.MaximumSelection = dto.MaximumSelection;
        group.Items = dto.Items.Select(item => new ProductAddonItemEntity { Id = Guid.NewGuid(), Name = item.Name.Trim(), Price = item.Price, DateCreated = DateTime.UtcNow }).ToList();
        await dbContext.SaveChangesAsync();
        return Ok(ToResponse(group));
    }

    private bool TryGetPartnerId(out Guid partnerId) => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub), out partnerId);
    private Task<bool> IsOwnedActiveCompany(Guid companyId, Guid partnerId) => dbContext.Companies.AnyAsync(x => x.Id == companyId && x.OwnerId == partnerId && x.Status == CompanyStatus.Approved && !x.IsDeleted);
    private Task<CompanyProductCategoryEntity?> FindCategory(Guid companyId, Guid categoryId, Guid partnerId) => dbContext.CompanyProductCategories.Include(x => x.Company).SingleOrDefaultAsync(x => x.Id == categoryId && x.CompanyId == companyId && !x.IsDeleted && !x.Company.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved);
    private Task<CompanyProductEntity?> FindProduct(Guid companyId, Guid productId, Guid partnerId) => dbContext.CompanyProducts.Include(x => x.Company).SingleOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId && !x.IsDeleted && !x.Company.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved);
    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string? NormalizeWeightType(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant() switch { "g" => "g", "ml" => "ml", _ => null };
    private static ProductCategoryResponseDto ToResponse(CompanyProductCategoryEntity category) => new(category.Id, category.CompanyId, category.Name, category.SortOrder, category.DateCreated);
    private static ProductResponseDto ToResponse(CompanyProductEntity product) => new(product.Id, product.CompanyId, product.CategoryId, product.Name, product.Description, product.Price, product.ImageUrl, product.Calories, product.WeightOrVolume, product.WeightType, product.DateCreated);
    private static ProductAddonGroupResponseDto ToResponse(ProductAddonGroupEntity group) => new(group.Id, group.ProductId, group.Name, group.MinimumSelection, group.MaximumSelection, group.Items.Where(x => !x.IsDeleted).Select(x => new ProductAddonItemResponseDto(x.Id, x.Name, x.Price)).ToList(), group.DateCreated);
}
