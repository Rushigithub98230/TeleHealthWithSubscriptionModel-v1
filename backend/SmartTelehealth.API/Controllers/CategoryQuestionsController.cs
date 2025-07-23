using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryQuestionsController : ControllerBase
    {
        private readonly ICategoryQuestionService _questionService;
        private readonly ICategoryQuestionAnswerService _answerService;
        public CategoryQuestionsController(ICategoryQuestionService questionService, ICategoryQuestionAnswerService answerService)
        {
            _questionService = questionService;
            _answerService = answerService;
        }

        // --- Superadmin: CRUD for questions ---
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllQuestions() => Ok(await _questionService.GetAllAsync());

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetQuestion(Guid id) => Ok(await _questionService.GetByIdAsync(id));

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateQuestion([FromBody] CategoryQuestion question)
        {
            await _questionService.AddAsync(question);
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] CategoryQuestion question)
        {
            if (id != question.Id) return BadRequest();
            await _questionService.UpdateAsync(question);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            await _questionService.DeleteAsync(id);
            return Ok();
        }

        // --- User: fetch questions for a category ---
        [HttpGet("by-category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuestionsByCategory(Guid categoryId) => Ok(await _questionService.GetByCategoryIdAsync(categoryId));

        // --- User: submit answers ---
        [HttpPost("answers")]
        [Authorize]
        public async Task<IActionResult> SubmitAnswer([FromBody] CategoryQuestionAnswer answer)
        {
            await _answerService.AddAsync(answer);
            return Ok();
        }

        // --- User: get answers by user/category ---
        [HttpGet("answers/by-user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetAnswersByUser(Guid userId) => Ok(await _answerService.GetByUserIdAsync(userId));

        [HttpGet("answers/by-category/{categoryId}")]
        [Authorize]
        public async Task<IActionResult> GetAnswersByCategory(Guid categoryId) => Ok(await _answerService.GetByCategoryIdAsync(categoryId));
    }
} 