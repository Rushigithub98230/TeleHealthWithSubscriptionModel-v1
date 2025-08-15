using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionnaireController : ControllerBase
    {
        private readonly IQuestionnaireService _questionnaireService;
        private readonly IFileStorageService _fileStorageService;

        public QuestionnaireController(IQuestionnaireService questionnaireService, IFileStorageService fileStorageService)
        {
            _questionnaireService = questionnaireService;
            _fileStorageService = fileStorageService;
        }

        // Template Management
        [HttpGet("templates")]
        public async Task<ActionResult<JsonModel>> GetAllTemplates()
        {
            try
            {
                var templates = await _questionnaireService.GetAllTemplatesAsync();
                return Ok(JsonModel>.SuccessResponse(templates.ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel>.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpGet("templates/{id}")]
        public async Task<ActionResult<JsonModel> GetTemplateById(Guid id)
        {
            try
            {
                var result = await _questionnaireService.GetTemplateByIdAsync(id);
                if (!result.Success)
                    return StatusCode(result.StatusCode, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpGet("templates/by-category/{categoryId}")]
        public async Task<ActionResult<JsonModel>> GetTemplatesByCategory(Guid categoryId)
        {
            try
            {
                var result = await _questionnaireService.GetTemplatesByCategoryAsync(categoryId);
                if (!result.Success)
                    return StatusCode(result.StatusCode, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel>.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpPost("templates")]
        public async Task<ActionResult<JsonModel> CreateTemplate([FromBody] CreateQuestionnaireTemplateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(JsonModel.ErrorResponse("Invalid request data", 400));

                var result = await _questionnaireService.CreateTemplateAsync(dto, new List<IFormFile>());
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpPost("templates/with-files")]
        public async Task<ActionResult<JsonModel> CreateTemplateWithFiles([FromForm] string templateJson, [FromForm] List<IFormFile> files)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templateJson))
                    return BadRequest(JsonModel.ErrorResponse("Template JSON is required", 400));

                CreateQuestionnaireTemplateDto dto;
                try
                {
                    dto = JsonConvert.DeserializeObject<CreateQuestionnaireTemplateDto>(templateJson);
                }
                catch (JsonException ex)
                {
                    return BadRequest(JsonModel.ErrorResponse($"Invalid JSON format: {ex.Message}", 400));
                }

                if (dto == null)
                    return BadRequest(JsonModel.ErrorResponse("Failed to deserialize template data", 400));

                var result = await _questionnaireService.CreateTemplateAsync(dto, files ?? new List<IFormFile>());
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpPut("templates/{id}")]
        public async Task<ActionResult<JsonModel> UpdateTemplate(Guid id, [FromBody] CreateQuestionnaireTemplateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(JsonModel.ErrorResponse("Invalid request data", 400));
                var result = await _questionnaireService.UpdateTemplateAsync(id, dto, new List<IFormFile>());
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpPut("templates/{id}/with-files")]
        public async Task<ActionResult<JsonModel> UpdateTemplateWithFiles(Guid id, [FromForm] string templateJson, [FromForm] List<IFormFile> files)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templateJson))
                    return BadRequest(JsonModel.ErrorResponse("Template JSON is required", 400));
                CreateQuestionnaireTemplateDto dto;
                try
                {
                    dto = JsonConvert.DeserializeObject<CreateQuestionnaireTemplateDto>(templateJson);
                }
                catch (JsonException ex)
                {
                    return BadRequest(JsonModel.ErrorResponse($"Invalid JSON format: {ex.Message}", 400));
                }
                if (dto == null)
                    return BadRequest(JsonModel.ErrorResponse("Failed to deserialize template data", 400));
                var result = await _questionnaireService.UpdateTemplateAsync(id, dto, files ?? new List<IFormFile>());
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpDelete("templates/{id}")]
        public async Task<ActionResult> DeleteTemplate(Guid id)
        {
            try
            {
                await _questionnaireService.DeleteTemplateAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonModel.ErrorResponse(ex.Message, 400));
            }
        }

        // User Response Management
        [HttpPost("responses")]
        public async Task<ActionResult<JsonModel> SubmitResponse([FromBody] CreateUserResponseDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(JsonModel.ErrorResponse("Invalid request data", 400));
                var result = await _questionnaireService.SubmitUserResponseAsync(dto);
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(JsonModel.ErrorResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpGet("responses/{id}")]
        public async Task<ActionResult<JsonModel> GetUserResponseById(Guid id)
        {
            try
            {
                var result = await _questionnaireService.GetUserResponseByIdAsync(id);
                if (!result.Success)
                    return StatusCode(result.StatusCode, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpGet("responses/user/{userId}")]
        public async Task<ActionResult<JsonModel>> GetUserResponsesByUserId(int userId)
        {
            try
            {
                var result = await _questionnaireService.GetUserResponsesByCategoryAsync(userId, Guid.Empty);
                if (!result.Success)
                    return StatusCode(result.StatusCode, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel>.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpGet("responses/{userId}/{templateId}")]
        public async Task<ActionResult<JsonModel> GetUserResponse(int userId, Guid templateId)
        {
            try
            {
                var result = await _questionnaireService.GetUserResponseAsync(userId, templateId);
                if (!result.Success)
                    return StatusCode(result.StatusCode, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }

        [HttpGet("responses/{userId}/by-category/{categoryId}")]
        public async Task<ActionResult<JsonModel>> GetUserResponsesByCategory(int userId, Guid categoryId)
        {
            try
            {
                var result = await _questionnaireService.GetUserResponsesByCategoryAsync(userId, categoryId);
                if (!result.Success)
                    return StatusCode(result.StatusCode, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonModel>.ErrorResponse($"Internal server error: {ex.Message}", 500));
            }
        }
    }
} 