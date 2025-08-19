using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
public class PrivilegesController : BaseController
{
    private readonly IPrivilegeService _privilegeService;

    public PrivilegesController(IPrivilegeService privilegeService)
    {
        _privilegeService = privilegeService;
    }

    [HttpGet]
    public async Task<JsonModel> GetAll()
    {
        return await _privilegeService.GetAllPrivilegesAsync(GetToken(HttpContext));
    }
} 