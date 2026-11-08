using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlovoBackEnd.Controllers.Client;
[ApiController]
[Authorize(Roles = "Client")]
[Route("api/client/products")]
public class ClientProductsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] Guid companyId) =>  Ok(await clientService.GetById(companyId));
    
}