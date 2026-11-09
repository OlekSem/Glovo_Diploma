using GlovoBackEnd.Data;
using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Domain.Enums;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Services.Implementations;

public class PartnerCompanyAccessService(AppDbContext dbContext) : IPartnerCompanyAccessService
{
    public async Task<bool> HasActiveCompanyAccessAsync(Guid companyId, Guid partnerId) =>
      await dbContext.Companies.AnyAsync(x => !x.IsDeleted && x.Id == companyId && x.OwnerId == partnerId && x.Status == CompanyStatus.Approved);

    public async Task<CompanyBranchEntity?> GetBranchAsync(Guid companyId, Guid branchId, Guid partnerId) =>
       await dbContext.CompanyBranches.Include(x => x.Company).SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == branchId && x.CompanyId == companyId && !x.Company.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved);

    public async Task<CompanyProductCategoryEntity?> GetCategoryAsync(Guid companyId, Guid categoryId, Guid partnerId) =>
      await  dbContext.CompanyProductCategories.Include(x => x.Company).SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == categoryId && x.CompanyId == companyId && !x.Company.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved);

    public async Task<CompanyProductEntity?> GetProductAsync(Guid companyId, Guid productId, Guid partnerId) =>
       await dbContext.CompanyProducts.Include(x => x.Company).SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == productId && x.CompanyId == companyId && !x.Company.IsDeleted && x.Company.OwnerId == partnerId && x.Company.Status == CompanyStatus.Approved);
}
