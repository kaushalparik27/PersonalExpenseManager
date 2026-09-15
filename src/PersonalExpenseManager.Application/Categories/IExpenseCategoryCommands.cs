using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.Application.Categories;

public interface IExpenseCategoryCommands
{
    Task<MutationResult<CategoryDto>> CreateCategoryAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<CategoryDto>> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<bool> ArchiveCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default);

    Task<bool> ReorderCategoriesAsync(Guid userId, ReorderRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<SubCategoryDto>> CreateSubCategoryAsync(Guid userId, Guid categoryId, CreateSubCategoryRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<SubCategoryDto>> UpdateSubCategoryAsync(Guid userId, Guid subCategoryId, UpdateSubCategoryRequest request, CancellationToken cancellationToken = default);

    Task<bool> ArchiveSubCategoryAsync(Guid userId, Guid subCategoryId, CancellationToken cancellationToken = default);

    Task<bool> ReorderSubCategoriesAsync(Guid userId, Guid categoryId, ReorderRequest request, CancellationToken cancellationToken = default);
}
