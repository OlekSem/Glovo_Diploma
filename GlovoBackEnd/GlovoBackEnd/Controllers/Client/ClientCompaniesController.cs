using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Models.Address;
using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlovoBackEnd.Controllers.Client;
[ApiController]
[Authorize(Roles = "Client")]
[Route("api/client/companies")]
public class ClientCompaniesController(IClientService clientService) : ControllerBase
{
    
    [HttpGet]
    public async Task<IActionResult> GetAllCompanies([FromQuery]ClientAddressDto clientAddressDto) => Ok(await clientService.GetAll(clientAddressDto));
    
}