using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionnaireController : ControllerBase
    {
        private readonly IQuestionnaireService _service;
        public QuestionnaireController(IQuestionnaireService service)
        {
            _service = service;
        }

        // --- Admin: CRUD for templates ---
        [HttpGet("templates")]
        [Authorize(Roles = "Admin,Superadmin")]
        public async Task<IActionResult> GetAllTemplates() => Ok(await _service.GetAllTemplatesAsync());

        [HttpGet("templates/{id}")]
        [Authorize(Roles = "Admin,Superadmin")]
        public async Task<IActionResult> GetTemplate(Guid id) => Ok(await _service.GetTemplateByIdAsync(id));

        [HttpPost("templates")]
        [Authorize(Roles = "Admin,Superadmin")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateQuestionnaireTemplateDto dto)
        {
            var id = await _service.CreateTemplateAsync(dto);
            return Ok(new { id });
        }

        [HttpPut("templates/{id}")]
        [Authorize(Roles = "Admin,Superadmin")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] CreateQuestionnaireTemplateDto dto)
        {
            await _service.UpdateTemplateAsync(id, dto);
            return Ok();
        }

        [HttpDelete("templates/{id}")]
        [Authorize(Roles = "Admin,Superadmin")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            await _service.DeleteTemplateAsync(id);
            return Ok();
        }

        // --- User: fetch questions for category/plan ---
        [HttpGet("by-category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTemplatesByCategory(Guid categoryId) => Ok(await _service.GetTemplatesByCategoryAsync(categoryId));

        // REMOVE: PlanId from all endpoints, parameters, and logic

        // --- User: submit and fetch responses ---
        [HttpPost("responses")]
        [Authorize]
        public async Task<IActionResult> SubmitUserResponse([FromBody] CreateUserResponseDto dto)
        {
            var id = await _service.SubmitUserResponseAsync(dto);
            return Ok(new { id });
        }

        [HttpGet("responses/{userId}/{templateId}")]
        [Authorize]
        public async Task<IActionResult> GetUserResponse(Guid userId, Guid templateId) => Ok(await _service.GetUserResponseAsync(userId, templateId));

        [HttpGet("responses/by-category/{userId}/{categoryId}")]
        [Authorize]
        public async Task<IActionResult> GetUserResponsesByCategory(Guid userId, Guid categoryId) => Ok(await _service.GetUserResponsesByCategoryAsync(userId, categoryId));
    }
} 