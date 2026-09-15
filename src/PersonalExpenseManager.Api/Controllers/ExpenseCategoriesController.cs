using Microsoft.AspNetCore.Mvc;
using PersonalExpenseManager.Api.Common;
using PersonalExpenseManager.Application.Categories;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.Api.Controllers;

[ApiController]
[Route("api/expense-categories")]
public class ExpenseCategoriesController(IExpenseCategoryQueries queries, IExpenseCategoryCommands commands) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await queries.GetForUserAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(categories));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.CreateCategoryAsync(User.GetUserId(), request, cancellationToken);
        if (result.Error is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(result.Error));
        }

        return Ok(ApiResponse<CategoryDto>.Ok(result.Value!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.UpdateCategoryAsync(User.GetUserId(), id, request, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Category not found."));
        }

        if (result.Error is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(result.Error));
        }

        return Ok(ApiResponse<CategoryDto>.Ok(result.Value!));
    }

    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> ArchiveCategory(Guid id, CancellationToken cancellationToken)
    {
        var archived = await commands.ArchiveCategoryAsync(User.GetUserId(), id, cancellationToken);
        if (!archived)
        {
            return NotFound(ApiResponse<object>.Fail("Category not found."));
        }

        return NoContent();
    }

    [HttpPatch("reorder")]
    public async Task<IActionResult> ReorderCategories(ReorderRequest request, CancellationToken cancellationToken)
    {
        var reordered = await commands.ReorderCategoriesAsync(User.GetUserId(), request, cancellationToken);
        if (!reordered)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail("One or more categories could not be found."));
        }

        return NoContent();
    }

    [HttpPost("{categoryId:guid}/subcategories")]
    public async Task<IActionResult> CreateSubCategory(Guid categoryId, CreateSubCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.CreateSubCategoryAsync(User.GetUserId(), categoryId, request, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Category not found."));
        }

        if (result.Error is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(result.Error));
        }

        return Ok(ApiResponse<SubCategoryDto>.Ok(result.Value!));
    }

    [HttpPut("subcategories/{id:guid}")]
    public async Task<IActionResult> UpdateSubCategory(Guid id, UpdateSubCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.UpdateSubCategoryAsync(User.GetUserId(), id, request, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Subcategory not found."));
        }

        if (result.Error is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(result.Error));
        }

        return Ok(ApiResponse<SubCategoryDto>.Ok(result.Value!));
    }

    [HttpPatch("subcategories/{id:guid}/archive")]
    public async Task<IActionResult> ArchiveSubCategory(Guid id, CancellationToken cancellationToken)
    {
        var archived = await commands.ArchiveSubCategoryAsync(User.GetUserId(), id, cancellationToken);
        if (!archived)
        {
            return NotFound(ApiResponse<object>.Fail("Subcategory not found."));
        }

        return NoContent();
    }

    [HttpPatch("{categoryId:guid}/subcategories/reorder")]
    public async Task<IActionResult> ReorderSubCategories(Guid categoryId, ReorderRequest request, CancellationToken cancellationToken)
    {
        var reordered = await commands.ReorderSubCategoriesAsync(User.GetUserId(), categoryId, request, cancellationToken);
        if (!reordered)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail("One or more subcategories could not be found."));
        }

        return NoContent();
    }
}
