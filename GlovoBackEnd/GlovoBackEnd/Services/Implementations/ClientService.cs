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
            .Where(x => x.Branches.Any(y =>
                y.Address == clientAddressDto.Address && y.Region == clientAddressDto.Region ))
            .Select(x => CompanyMapper.ToClientCompanyResponseDto(x))
            .ToListAsync();
        return companies;
        
    }

    public async Task<IReadOnlyList<ClientProductResponseDto>> GetById(Guid companyId)
    {
        var products = await dbContext.CompanyProducts
            .Where(x => x.CompanyId == companyId)
            .Select(x => CompanyMapper.ToClientProductResponseDto(x))
            .ToListAsync();
        return products;
    }
}