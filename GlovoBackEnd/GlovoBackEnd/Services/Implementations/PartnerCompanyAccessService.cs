using GlovoBackEnd.Data;
using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Domain.Enums;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Services.Implementations;

public class PartnerCompanyAccessService(AppDbContext dbContext) : IPartnerCompanyAccessService
{
    public Task<bool> HasActiveCompanyAccessAsync(Guid companyId, Guid partnerId) =>
        dbContext.Companies.AnyAsync(x => !x.IsDeleted && x.Id == companyId && x.OwnerId == partnerId && x.Status == CompanyStatus.Approved);

    public Task<CompanyBranchEntity?> GetBranchAsync(Guid companyId, Guid branchId, Guid partnerId) =>
        dbContext.CompanyBranches.Include(x => x.Company).SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == branchId && x.CompanyId == companyId && !x.Company.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved);

    public Task<CompanyProductCategoryEntity?> GetCategoryAsync(Guid companyId, Guid categoryId, Guid partnerId) =>
        dbContext.CompanyProductCategories.Include(x => x.Company).SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == categoryId && x.CompanyId == companyId && !x.Company.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved);

    public Task<CompanyProductEntity?> GetProductAsync(Guid companyId, Guid productId, Guid partnerId) =>
        dbContext.CompanyProducts.Include(x => x.Company).SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == productId && x.CompanyId == companyId && !x.Company.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved);
}
