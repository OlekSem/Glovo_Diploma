using GlovoBackEnd.Models.Address;
using GlovoBackEnd.Models.Dtos.Company;

namespace GlovoBackEnd.Services.Interfaces;

public interface IClientService
{
    Task<IReadOnlyList<ClientCompanyResponseDto>> GetAll(ClientAddressDto clientAddressDto);
    Task<IReadOnlyList<ClientProductResponseDto>> GetById(Guid companyId);
}