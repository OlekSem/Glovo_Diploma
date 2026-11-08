using GlovoBackEnd.Domain.Entities.Company;
using GlovoBackEnd.Models.Dtos.Company;

namespace GlovoBackEnd.Mappers;

public static class CompanyMapper
{
    public static ClientCompanyResponseDto ToClientCompanyResponseDto(this CompanyEntity company) =>
        new(company.Id, company.Name, company.Description,
            company.Category, company.Subcategory, company.IconUrl,
            company.BannerUrl, company.Branches.Count);
    public static ClientProductResponseDto ToClientProductResponseDto(this CompanyProductEntity product) =>
        new(product.Id, product.CompanyId, product.CategoryId,product.Name,
            product.Description, product.Price, product.ImageUrl,
            product.Calories, product.WeightOrVolume,  product.WeightType);

}