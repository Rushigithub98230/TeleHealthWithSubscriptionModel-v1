using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProviders()
    {
        var response = await _providerService.GetAllProvidersAsync();
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProvider(int id)
    {
        var response = await _providerService.GetProviderByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProvider([FromBody] CreateProviderDto createProviderDto)
    {
        var response = await _providerService.CreateProviderAsync(createProviderDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProvider(int id, [FromBody] UpdateProviderDto updateProviderDto)
    {
        if (id != updateProviderDto.Id)
            return BadRequest("ID mismatch");
        var response = await _providerService.UpdateProviderAsync(id, updateProviderDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProvider(int id)
    {
        var response = await _providerService.DeleteProviderAsync(id);
        return StatusCode(response.StatusCode, response);
    }
} 