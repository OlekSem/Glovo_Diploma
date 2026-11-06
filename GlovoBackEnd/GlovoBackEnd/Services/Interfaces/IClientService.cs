using GlovoBackEnd.Models.Dtos.Company;

namespace GlovoBackEnd.Services.Interfaces;

public interface IClientService
{
    Task<IReadOnlyList<ClientCompanyResponseDto>> GetAll(string address);
}