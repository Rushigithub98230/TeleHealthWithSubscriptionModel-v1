using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<JsonModel>> GetAllCategories()
    {
        var response = await _categoryService.GetAllCategoriesAsync();
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JsonModel>> GetCategory(Guid id)
    {
        var response = await _categoryService.GetCategoryAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<JsonModel>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        var response = await _categoryService.CreateCategoryAsync(createCategoryDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JsonModel>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        if (!Guid.TryParse(updateCategoryDto.Id, out var dtoId) || id != dtoId)
            return BadRequest(new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 });
        var response = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<JsonModel>> DeleteCategory(Guid id)
    {
        var response = await _categoryService.DeleteCategoryAsync(id);
        return StatusCode(response.StatusCode, response);
    }
} 