using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

public class InfermedicaService : IInfermedicaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InfermedicaService> _logger;
    private readonly string _appId;
    private readonly string _appKey;
    private readonly string _baseUrl;

    public InfermedicaService(IConfiguration config, ILogger<InfermedicaService> logger)
    {
        _logger = logger;
        _appId = config["Infermedica:AppId"] ?? throw new InvalidOperationException("Infermedica AppId missing");
        _appKey = config["Infermedica:AppKey"] ?? throw new InvalidOperationException("Infermedica AppKey missing");
        _baseUrl = config["Infermedica:BaseUrl"] ?? "https://api.infermedica.com/v3";
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("App-Id", _appId);
        _httpClient.DefaultRequestHeaders.Add("App-Key", _appKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<InfermedicaParseResponseDto> ParseAsync(string text)
    {
        var payload = JsonSerializer.Serialize(new { text });
        var response = await _httpClient.PostAsync(_baseUrl + "/parse", new StringContent(payload, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<InfermedicaParseResponseDto>(json) ?? new();
    }

    public async Task<InfermedicaDiagnosisResponseDto> DiagnoseAsync(InfermedicaDiagnosisRequestDto request)
    {
        var payload = JsonSerializer.Serialize(request);
        var response = await _httpClient.PostAsync(_baseUrl + "/diagnosis", new StringContent(payload, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<InfermedicaDiagnosisResponseDto>(json) ?? new();
    }

    public async Task<InfermedicaSuggestSpecialistResponseDto> SuggestSpecialistAsync(InfermedicaDiagnosisRequestDto request)
    {
        var payload = JsonSerializer.Serialize(request);
        var response = await _httpClient.PostAsync(_baseUrl + "/suggest-specialist", new StringContent(payload, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<InfermedicaSuggestSpecialistResponseDto>(json) ?? new();
    }
} 