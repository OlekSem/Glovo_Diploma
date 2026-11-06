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
            Status = CompanyStatus.Pending,
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
            .Where(x => !x.IsDeleted && x.OwnerId == partnerId && x.Status == CompanyStatus.Approved)
            .OrderBy(x => x.Name)
            .Select(x => new CompanyResponseDto(x.Id, x.Name, x.Description, x.OwnerId, x.DateCreated, x.Status, x.ModerationComment, x.ReviewedAt, x.Category, x.Subcategory, x.IconUrl, x.BannerUrl))
            .ToListAsync();
        return Ok(companies);
    }

    /// <summary>Returns the partner's submissions, including those still being moderated.</summary>
    [HttpGet("mine/requests")]
    public async Task<ActionResult<IReadOnlyCollection<CompanyResponseDto>>> GetMyRequests()
    {
        if (!TryGetPartnerId(out var partnerId))
            return Unauthorized(new { message = "Invalid partner session." });

        var companies = await dbContext.Companies
            .Where(x => !x.IsDeleted && x.OwnerId == partnerId)
            .OrderByDescending(x => x.DateCreated)
            .Select(x => new CompanyResponseDto(x.Id, x.Name, x.Description, x.OwnerId, x.DateCreated, x.Status, x.ModerationComment, x.ReviewedAt, x.Category, x.Subcategory, x.IconUrl, x.BannerUrl))
            .ToListAsync();
        return Ok(companies);
    }

    [HttpPut("{companyId:guid}")]
    public async Task<ActionResult<CompanyResponseDto>> Update(Guid companyId, [FromBody] UpdateCompanyDto dto)
    {
        if (!TryGetPartnerId(out var partnerId))
            return Unauthorized(new { message = "Invalid partner session." });

        var company = await dbContext.Companies.SingleOrDefaultAsync(x =>
            !x.IsDeleted && x.Id == companyId && x.OwnerId == partnerId && x.Status == CompanyStatus.Approved);
        if (company is null)
            return NotFound(new { message = "Company not found or is not active." });

        var normalizedName = NormalizeName(dto.Name);
        var duplicateName = await dbContext.Companies.AnyAsync(x =>
            !x.IsDeleted && x.OwnerId == partnerId && x.Id != companyId && x.NormalizedName == normalizedName);
        if (duplicateName)
            return BadRequest(new { message = "You already have a company with this name." });

        company.Name = dto.Name.Trim();
        company.NormalizedName = normalizedName;
        company.Description = dto.Description.Trim();
        company.Category = TrimToNull(dto.Category);
        company.Subcategory = TrimToNull(dto.Subcategory);
        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(company));
    }

    [HttpGet("{companyId:guid}/branches")]
    public async Task<ActionResult<IReadOnlyCollection<CompanyBranchResponseDto>>> GetBranches(Guid companyId)
    {
        if (!TryGetPartnerId(out var partnerId))
            return Unauthorized(new { message = "Invalid partner session." });

        if (!await IsApprovedOwnedCompany(companyId, partnerId))
            return NotFound(new { message = "Company not found or is not active." });

        var branches = await dbContext.CompanyBranches
            .Where(x => !x.IsDeleted && x.CompanyId == companyId)
            .OrderByDescending(x => x.DateCreated)
            .Select(x => new CompanyBranchResponseDto(
                x.Id, x.CompanyId, x.Phone, x.Email, x.Locality, x.Region, x.Address, x.PostalCode, x.DateCreated))
            .ToListAsync();
        return Ok(branches);
    }

    [HttpPost("{companyId:guid}/branches")]
    public async Task<ActionResult<CompanyBranchResponseDto>> CreateBranch(Guid companyId, [FromBody] CreateCompanyBranchDto dto)
    {
        if (!TryGetPartnerId(out var partnerId))
            return Unauthorized(new { message = "Invalid partner session." });

        if (!await IsApprovedOwnedCompany(companyId, partnerId))
            return NotFound(new { message = "Company not found or is not active." });

        var branch = new CompanyBranchEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim(),
            Locality = dto.Locality.Trim(),
            Region = dto.Region.Trim(),
            Address = dto.Address.Trim(),
            PostalCode = dto.PostalCode.Trim(),
            DateCreated = DateTime.UtcNow
        };
        dbContext.CompanyBranches.Add(branch);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBranches), new { companyId }, ToBranchResponse(branch));
    }

    private bool TryGetPartnerId(out Guid partnerId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(value, out partnerId);
    }

    private static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Task<bool> IsApprovedOwnedCompany(Guid companyId, Guid partnerId) =>
        dbContext.Companies.AnyAsync(x => !x.IsDeleted && x.Id == companyId && x.OwnerId == partnerId && x.Status == CompanyStatus.Approved);

    private static CompanyResponseDto ToResponse(CompanyEntity company) =>
        new(company.Id, company.Name, company.Description, company.OwnerId, company.DateCreated, company.Status, company.ModerationComment, company.ReviewedAt, company.Category, company.Subcategory, company.IconUrl, company.BannerUrl);

    private static CompanyBranchResponseDto ToBranchResponse(CompanyBranchEntity branch) =>
        new(branch.Id, branch.CompanyId, branch.Phone, branch.Email, branch.Locality, branch.Region, branch.Address, branch.PostalCode, branch.DateCreated);
}
