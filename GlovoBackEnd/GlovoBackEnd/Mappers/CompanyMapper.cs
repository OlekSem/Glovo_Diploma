using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Models.Dtos.Company;

namespace GlovoBackEnd.Mappers;

public static class CompanyMapper
{
    public static ClientCompanyResponseDto ToClientCompanyResponseDto(this CompanyEntity company) =>
        new(company.Id, company.Name, company.Description,
            company.Category, company.Subcategory, company.IconUrl,
            company.BannerUrl, company.Branches.Count);

}