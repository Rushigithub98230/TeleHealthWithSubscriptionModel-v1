using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InfermedicaController : ControllerBase
{
    private readonly IInfermedicaService _infermedicaService;
    private readonly IProviderRepository _providerRepository;

    public InfermedicaController(IInfermedicaService infermedicaService, IProviderRepository providerRepository)
    {
        _infermedicaService = infermedicaService;
        _providerRepository = providerRepository;
    }

    [HttpPost("parse")]
    public async Task<ActionResult<InfermedicaParseResponseDto>> Parse([FromBody] string text)
    {
        var result = await _infermedicaService.ParseAsync(text);
        return Ok(result);
    }

    [HttpPost("diagnose")]
    public async Task<ActionResult<InfermedicaDiagnosisResponseDto>> Diagnose([FromBody] InfermedicaDiagnosisRequestDto request)
    {
        var result = await _infermedicaService.DiagnoseAsync(request);
        return Ok(result);
    }

    [HttpPost("recommend-doctors")]
    public async Task<ActionResult<IEnumerable<object>>> RecommendDoctors([FromBody] InfermedicaDiagnosisRequestDto request)
    {
        var specialistResult = await _infermedicaService.SuggestSpecialistAsync(request);
        if (specialistResult.Specialties.Count == 0)
            return Ok(new List<object>());
        var specialty = specialistResult.Specialties[0].Name;
        var providers = await _providerRepository.GetProvidersBySpecialtyAsync(specialty);
        return Ok(providers.Select(p => new { p.Id, p.FullName, p.Specialty, p.Email, p.PhoneNumber }));
    }
} 