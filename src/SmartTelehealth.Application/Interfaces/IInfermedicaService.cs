using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IInfermedicaService
{
    Task<InfermedicaParseResponseDto> ParseAsync(string text);
    Task<InfermedicaDiagnosisResponseDto> DiagnoseAsync(InfermedicaDiagnosisRequestDto request);
    Task<InfermedicaSuggestSpecialistResponseDto> SuggestSpecialistAsync(InfermedicaDiagnosisRequestDto request);
} 