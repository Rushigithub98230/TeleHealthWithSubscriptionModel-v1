using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InfermedicaController : BaseController
{
    private readonly IInfermedicaService _infermedicaService;
    private readonly IProviderRepository _providerRepository;

    public InfermedicaController(IInfermedicaService infermedicaService, IProviderRepository providerRepository)
    {
        _infermedicaService = infermedicaService;
        _providerRepository = providerRepository;
    }

    [HttpPost("parse")]
    public async Task<JsonModel> Parse([FromBody] string text)
    {
        var result = await _infermedicaService.ParseAsync(text);
        return new JsonModel { data = result, Message = "Text parsed successfully", StatusCode = 200 };
    }

    [HttpPost("diagnose")]
    public async Task<JsonModel> Diagnose([FromBody] InfermedicaDiagnosisRequestDto request)
    {
        var result = await _infermedicaService.DiagnoseAsync(request);
        return new JsonModel { data = result, Message = "Diagnosis completed successfully", StatusCode = 200 };
    }

    [HttpPost("recommend-doctors")]
    public async Task<JsonModel> RecommendDoctors([FromBody] InfermedicaDiagnosisRequestDto request)
    {
        var specialistResult = await _infermedicaService.SuggestSpecialistAsync(request);
        if (specialistResult.Specialties.Count == 0)
            return new JsonModel { data = new List<object>(), Message = "No specialists found", StatusCode = 200 };
        
        var specialty = specialistResult.Specialties[0].Name;
        var providers = await _providerRepository.GetProvidersBySpecialtyAsync(specialty);
        var providerData = providers.Select(p => new { p.Id, p.FullName, p.Specialty, p.Email, p.PhoneNumber });
        
        return new JsonModel { data = providerData, Message = "Doctors recommended successfully", StatusCode = 200 };
    }
} 