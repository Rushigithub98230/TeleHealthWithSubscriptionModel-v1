using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Superadmin")]
public class PrivilegesController : ControllerBase
{
    private readonly IPrivilegeService _privilegeService;

    public PrivilegesController(IPrivilegeService privilegeService)
    {
        _privilegeService = privilegeService;
    }

    [HttpGet]
    public async Task<ActionResult<JsonModel>> GetAll()
    {
        var response = await _privilegeService.GetAllPrivilegesAsync();
        return StatusCode(response.StatusCode, response);
    }
} 