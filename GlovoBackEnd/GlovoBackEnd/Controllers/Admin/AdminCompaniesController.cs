using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GlovoBackEnd.Data;
using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Domain.Enums;
using GlovoBackEnd.Models.Dtos.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Controllers.Admin;

[ApiController]
[Authorize(Roles = ApplicationRoleNames.Administrator)]
[Route("api/admin/companies")]
public class AdminCompaniesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AdminCompanyResponseDto>>> GetAll([FromQuery] CompanyStatus? status)
    {
        var query = dbContext.Companies.AsNoTracking().Where(x => !x.IsDeleted);
        if (status is not null)
            query = query.Where(x => x.Status == status);

        return Ok(await query
            .Include(x => x.Owner)
            .OrderBy(x => x.Status == CompanyStatus.Pending ? 0 : 1)
            .ThenByDescending(x => x.DateCreated)
            .Select(x => ToResponse(x))
            .ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminCompanyResponseDto>> GetById(Guid id)
    {
        var company = await dbContext.Companies.AsNoTracking()
            .Include(x => x.Owner)
            .SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == id);
        return company is null ? NotFound(new { message = "Company request not found." }) : Ok(ToResponse(company));
    }

    [HttpPost("{id:guid}/approve")]
    public Task<ActionResult<AdminCompanyResponseDto>> Approve(Guid id, [FromBody] ModerateCompanyDto dto) =>
        Moderate(id, CompanyStatus.Approved, dto);

    [HttpPost("{id:guid}/cancel")]
    public Task<ActionResult<AdminCompanyResponseDto>> Cancel(Guid id, [FromBody] ModerateCompanyDto dto) =>
        Moderate(id, CompanyStatus.Canceled, dto);

    private async Task<ActionResult<AdminCompanyResponseDto>> Moderate(Guid id, CompanyStatus decision, ModerateCompanyDto dto)
    {
        var company = await dbContext.Companies.Include(x => x.Owner)
            .SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == id);
        if (company is null)
            return NotFound(new { message = "Company request not found." });
        if (company.Status != CompanyStatus.Pending)
            return Conflict(new { message = "Only pending company requests can be moderated." });

        company.Status = decision;
        company.ModerationComment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = CurrentAdminId();
        await dbContext.SaveChangesAsync();
        return Ok(ToResponse(company));
    }

    private Guid? CurrentAdminId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static AdminCompanyResponseDto ToResponse(CompanyEntity company) => new(
        company.Id, company.Name, company.Description, company.Status, company.ModerationComment,
        company.DateCreated, company.ReviewedAt, company.OwnerId,
        $"{company.Owner.FirstName} {company.Owner.LastName}".Trim(), company.Owner.Email, company.Owner.Phone);
}
