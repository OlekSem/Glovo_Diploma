using GlovoBackEnd.Data;
using GlovoBackEnd.Mappers;
using GlovoBackEnd.Models.Address;
using GlovoBackEnd.Models.Dtos.Client;
using GlovoBackEnd.Models.Dtos.Company;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Services.Implementations;

public class ClientService(AppDbContext dbContext) : IClientService
{
    public async Task<IReadOnlyList<ClientCompanyResponseDto>> GetAll(ClientAddressDto clientAddressDto)
    {
        var companies = await dbContext.Companies
            .Include(c => c.Branches)
            .Where(x => 
                x.Branches.Any(y =>
                y.Region == clientAddressDto.Region
                && y.Locality == clientAddressDto.Locality))
            .Select(x =>
                CompanyMapper.ToClientCompanyResponseDto(x,
                x.Branches.Where(y =>
                    y.Region == clientAddressDto.Region && y.Locality == clientAddressDto.Locality)))
            .ToListAsync();
        return companies;
        
    }

    public async Task<IReadOnlyList<ClientProductResponseDto>>
        GetProductsByBranchesId(List<Guid> branchIds)
    {
        var branchProducts = await dbContext.BranchProducts
            .Where(x => branchIds.Contains(x.BranchId) && x.IsAvailable)
            .Include(x => x.Product)
            .ToListAsync();

        return branchProducts
            .GroupBy(x => x.ProductId)
            .Select(g => g.ToClientProductResponseDto())
            .ToList();
    }
}