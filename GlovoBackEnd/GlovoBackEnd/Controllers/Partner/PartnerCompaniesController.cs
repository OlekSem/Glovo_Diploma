using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GlovoBackEnd.Data;
using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Models.Dtos.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Controllers.Partner;

[ApiController]
[Authorize(Roles = PartnerRoleNames.CompanyOwner)]
[Route("api/partner/companies")]
public class PartnerCompaniesController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CompanyResponseDto>> Create([FromBody] CreateCompanyDto dto)
    {
        if (!TryGetPartnerId(out var partnerId))
            return Unauthorized(new { message = "Invalid partner session." });

        var partnerExists = await dbContext.Partners.AnyAsync(x => !x.IsDeleted && x.Id == partnerId);
        if (!partnerExists)
            return Unauthorized(new { message = "Partner account not found." });

        var normalizedName = NormalizeName(dto.Name);
        var exists = await dbContext.Companies.AnyAsync(x =>
            !x.IsDeleted && x.OwnerId == partnerId && x.NormalizedName == normalizedName);
        if (exists)
            return BadRequest(new { message = "You already have a company with this name." });

        var company = new CompanyEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = partnerId,
            Name = dto.Name.Trim(),
            NormalizedName = normalizedName,
            Description = dto.Description.Trim(),
            DateCreated = DateTime.UtcNow
        };
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMine), new { }, ToResponse(company));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyCollection<CompanyResponseDto>>> GetMine()
    {
        if (!TryGetPartnerId(out var partnerId))
            return Unauthorized(new { message = "Invalid partner session." });

        var companies = await dbContext.Companies
            .Where(x => !x.IsDeleted && x.OwnerId == partnerId)
            .OrderBy(x => x.Name)
            .Select(x => new CompanyResponseDto(x.Id, x.Name, x.Description, x.OwnerId, x.DateCreated))
            .ToListAsync();
        return Ok(companies);
    }

    private bool TryGetPartnerId(out Guid partnerId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(value, out partnerId);
    }

    private static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private static CompanyResponseDto ToResponse(CompanyEntity company) =>
        new(company.Id, company.Name, company.Description, company.OwnerId, company.DateCreated);
}
