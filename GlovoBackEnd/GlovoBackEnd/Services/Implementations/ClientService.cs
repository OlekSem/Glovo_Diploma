using GlovoBackEnd.Data;
using GlovoBackEnd.Mappers;
using GlovoBackEnd.Models.Dtos.Client;
using GlovoBackEnd.Models.Dtos.Company;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlovoBackEnd.Services.Implementations;

public class ClientService(AppDbContext dbContext) : IClientService
{
    public async Task<IReadOnlyList<ClientCompanyResponseDto>> GetAll(string address)
    {
        var companies = await dbContext.Companies
            .Include(c => c.Branches)
            .Where(x => x.Branches.Any(y => y.Address == address))
            .Select(x => CompanyMapper.ToClientCompanyResponseDto(x))
            .ToListAsync();
        return companies;
        
    }
}