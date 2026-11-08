using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GlovoBackEnd.Data;
using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Domain.Enums;
using GlovoBackEnd.Models.Dtos.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Controllers.Partner;

[ApiController]
[Authorize(Roles = PartnerRoleNames.CompanyOwner)]
[Route("api/partner/companies/{companyId:guid}/branches/{branchId:guid}/menu")]
public class PartnerBranchMenuController(AppDbContext db) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyCollection<BranchMenuCategoryDto>>> Categories(Guid companyId, Guid branchId)
    {
        if (!TryPartner(out var partnerId)) return Unauthorized();
        if (!await BranchExists(companyId, branchId, partnerId)) return NotFound();
        var states = db.BranchProductCategories.Where(x => x.BranchId == branchId && !x.IsDeleted);
        return Ok(await db.CompanyProductCategories.Where(x => x.CompanyId == companyId && !x.IsDeleted).OrderBy(x => x.SortOrder).Select(x => new BranchMenuCategoryDto(x.Id, x.Name, states.Where(s => s.CategoryId == x.Id).Select(s => (bool?)s.IsAvailable).FirstOrDefault() ?? false)).ToListAsync());
    }

    [HttpPut("categories/{categoryId:guid}")]
    public async Task<IActionResult> SetCategory(Guid companyId, Guid branchId, Guid categoryId, SetAvailabilityDto dto)
    {
        if (!TryPartner(out var partnerId)) return Unauthorized();
        if (!await BranchExists(companyId, branchId, partnerId) || !await db.CompanyProductCategories.AnyAsync(x => x.Id == categoryId && x.CompanyId == companyId && !x.IsDeleted)) return NotFound();
        var state = await db.BranchProductCategories.SingleOrDefaultAsync(x => x.BranchId == branchId && x.CategoryId == categoryId);
        if (state is null) { state = new BranchProductCategoryEntity { Id = Guid.NewGuid(), BranchId = branchId, CategoryId = categoryId, IsAvailable = dto.IsAvailable, DateCreated = DateTime.UtcNow }; db.BranchProductCategories.Add(state); }
        else state.IsAvailable = dto.IsAvailable;
        if (dto.IsAvailable)
        {
            var products = await db.CompanyProducts.Where(x => x.CompanyId == companyId && x.CategoryId == categoryId && !x.IsDeleted).ToListAsync();
            var assigned = await db.BranchProducts.Where(x => x.BranchId == branchId).Select(x => x.ProductId).ToListAsync();
            db.BranchProducts.AddRange(products.Where(x => !assigned.Contains(x.Id)).Select(x => new BranchProductEntity { Id = Guid.NewGuid(), BranchId = branchId, ProductId = x.Id, IsAvailable = true, DateCreated = DateTime.UtcNow }));
        }
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("categories/{categoryId:guid}/products")]
    public async Task<ActionResult<IReadOnlyCollection<BranchMenuProductDto>>> Products(Guid companyId, Guid branchId, Guid categoryId)
    {
        if (!TryPartner(out var partnerId)) return Unauthorized();
        if (!await BranchExists(companyId, branchId, partnerId)) return NotFound();
        var categoryOn = await db.BranchProductCategories.AnyAsync(x => x.BranchId == branchId && x.CategoryId == categoryId && x.IsAvailable && !x.IsDeleted);
        var states = db.BranchProducts.Where(x => x.BranchId == branchId && !x.IsDeleted);
        return Ok(await db.CompanyProducts.Where(x => x.CompanyId == companyId && x.CategoryId == categoryId && !x.IsDeleted).Select(x => new BranchMenuProductDto(x.Id, x.Name, x.Description, x.Price, x.ImageUrl, categoryOn && (states.Where(s => s.ProductId == x.Id).Select(s => (bool?)s.IsAvailable).FirstOrDefault() ?? false))).ToListAsync());
    }

    [HttpPut("products/{productId:guid}")]
    public async Task<IActionResult> SetProduct(Guid companyId, Guid branchId, Guid productId, SetAvailabilityDto dto)
    {
        if (!TryPartner(out var partnerId)) return Unauthorized();
        if (!await BranchExists(companyId, branchId, partnerId)) return NotFound();
        var product = await db.CompanyProducts.SingleOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId && !x.IsDeleted); if (product is null) return NotFound();
        var categoryOn = await db.BranchProductCategories.AnyAsync(x => x.BranchId == branchId && x.CategoryId == product.CategoryId && x.IsAvailable && !x.IsDeleted); if (!categoryOn) return BadRequest(new { message = "Enable the product category first." });
        var state = await db.BranchProducts.SingleOrDefaultAsync(x => x.BranchId == branchId && x.ProductId == productId); if (state is null) db.BranchProducts.Add(new BranchProductEntity { Id = Guid.NewGuid(), BranchId = branchId, ProductId = productId, IsAvailable = dto.IsAvailable, DateCreated = DateTime.UtcNow }); else state.IsAvailable = dto.IsAvailable;
        await db.SaveChangesAsync(); return NoContent();
    }

    private bool TryPartner(out Guid partnerId) => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub), out partnerId);
    private Task<bool> BranchExists(Guid companyId, Guid branchId, Guid partnerId) => db.CompanyBranches.AnyAsync(x => x.Id == branchId && x.CompanyId == companyId && !x.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved && !x.Company.IsDeleted);
}
