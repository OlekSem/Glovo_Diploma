using GlovoBackEnd.Domain.Entities.Company;

namespace GlovoBackEnd.Services.Interfaces;

public interface IPartnerCompanyAccessService
{
    Task<bool> HasActiveCompanyAccessAsync(Guid companyId, Guid partnerId);
    Task<CompanyBranchEntity?> GetBranchAsync(Guid companyId, Guid branchId, Guid partnerId);
    Task<CompanyProductCategoryEntity?> GetCategoryAsync(Guid companyId, Guid categoryId, Guid partnerId);
    Task<CompanyProductEntity?> GetProductAsync(Guid companyId, Guid productId, Guid partnerId);
}
