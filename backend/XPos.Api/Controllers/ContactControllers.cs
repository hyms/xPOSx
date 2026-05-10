using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _clientRepository;
    public ClientsController(IClientRepository clientRepository) { _clientRepository = clientRepository; }

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _clientRepository.GetAllAsync());
    [HttpGet("{id}")] public async Task<IActionResult> GetById(long id) { var client = await _clientRepository.GetByIdAsync(id); return client == null ? NotFound() : Ok(client); }
    [HttpPost] public async Task<IActionResult> Create(Client client) { var id = await _clientRepository.CreateAsync(client); client.Id = id; return CreatedAtAction(nameof(GetById), new { id }, client); }
    [HttpPut("{id}")] public async Task<IActionResult> Update(long id, Client client) { if (id != client.Id) return BadRequest(); return await _clientRepository.UpdateAsync(client) ? NoContent() : NotFound(); }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(long id) => await _clientRepository.DeleteAsync(id) ? NoContent() : NotFound();
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderRepository _providerRepository;
    public ProvidersController(IProviderRepository providerRepository) { _providerRepository = providerRepository; }

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _providerRepository.GetAllAsync());
    [HttpGet("{id}")] public async Task<IActionResult> GetById(long id) { var provider = await _providerRepository.GetByIdAsync(id); return provider == null ? NotFound() : Ok(provider); }
    [HttpPost] public async Task<IActionResult> Create(Provider provider) { var id = await _providerRepository.CreateAsync(provider); provider.Id = id; return CreatedAtAction(nameof(GetById), new { id }, provider); }
    [HttpPut("{id}")] public async Task<IActionResult> Update(long id, Provider provider) { if (id != provider.Id) return BadRequest(); return await _providerRepository.UpdateAsync(provider) ? NoContent() : NotFound(); }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(long id) => await _providerRepository.DeleteAsync(id) ? NoContent() : NotFound();
}
