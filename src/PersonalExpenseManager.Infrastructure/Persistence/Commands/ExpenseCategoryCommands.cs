using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Categories;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Persistence.Queries;

namespace PersonalExpenseManager.Infrastructure.Persistence.Commands;

public class ExpenseCategoryCommands(ApplicationDbContext db) : IExpenseCategoryCommands
{
    public async Task<MutationResult<CategoryDto>> CreateCategoryAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var nameTaken = await db.ExpenseCategories.AnyAsync(c => c.UserId == userId && c.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            return MutationResult<CategoryDto>.Fail("A category with this name already exists.");
        }

        var nextDisplayOrder = 1 + (await db.ExpenseCategories
            .Where(c => c.UserId == userId)
            .Select(c => (int?)c.DisplayOrder)
            .MaxAsync(cancellationToken) ?? -1);

        var category = new ExpenseCategory
        {
            UserId = userId,
            Name = request.Name,
            Icon = request.Icon,
            DisplayOrder = nextDisplayOrder,
        };

        db.ExpenseCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<CategoryDto>.Ok(ExpenseCategoryQueries.ToDto(category));
    }

    public async Task<MutationResult<CategoryDto>> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await db.ExpenseCategories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);

        if (category is null)
        {
            return MutationResult<CategoryDto>.NotFound();
        }

        var nameTaken = await db.ExpenseCategories
            .AnyAsync(c => c.UserId == userId && c.Id != categoryId && c.Name == request.Name, cancellationToken);

        if (nameTaken)
        {
            return MutationResult<CategoryDto>.Fail("A category with this name already exists.");
        }

        category.Name = request.Name;
        category.Icon = request.Icon;
        category.IsArchived = request.IsArchived;
        category.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<CategoryDto>.Ok(ExpenseCategoryQueries.ToDto(category));
    }

    public async Task<bool> ArchiveCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await db.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);
        if (category is null)
        {
            return false;
        }

        category.IsArchived = true;
        category.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ReorderCategoriesAsync(Guid userId, ReorderRequest request, CancellationToken cancellationToken = default)
    {
        var categories = await db.ExpenseCategories
            .Where(c => c.UserId == userId && request.OrderedIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (categories.Count != request.OrderedIds.Count)
        {
            return false;
        }

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            var category = categories.First(c => c.Id == request.OrderedIds[i]);
            category.DisplayOrder = i;
            category.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<MutationResult<SubCategoryDto>> CreateSubCategoryAsync(Guid userId, Guid categoryId, CreateSubCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var categoryExists = await db.ExpenseCategories.AnyAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);
        if (!categoryExists)
        {
            return MutationResult<SubCategoryDto>.NotFound();
        }

        var nameTaken = await db.ExpenseSubCategories
            .AnyAsync(s => s.ExpenseCategoryId == categoryId && s.Name == request.Name, cancellationToken);

        if (nameTaken)
        {
            return MutationResult<SubCategoryDto>.Fail("A subcategory with this name already exists in this category.");
        }

        var nextDisplayOrder = 1 + (await db.ExpenseSubCategories
            .Where(s => s.ExpenseCategoryId == categoryId)
            .Select(s => (int?)s.DisplayOrder)
            .MaxAsync(cancellationToken) ?? -1);

        var subCategory = new ExpenseSubCategory
        {
            UserId = userId,
            ExpenseCategoryId = categoryId,
            Name = request.Name,
            Icon = request.Icon,
            DisplayOrder = nextDisplayOrder,
        };

        db.ExpenseSubCategories.Add(subCategory);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<SubCategoryDto>.Ok(ToDto(subCategory));
    }

    public async Task<MutationResult<SubCategoryDto>> UpdateSubCategoryAsync(Guid userId, Guid subCategoryId, UpdateSubCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var subCategory = await db.ExpenseSubCategories.FirstOrDefaultAsync(s => s.Id == subCategoryId && s.UserId == userId, cancellationToken);
        if (subCategory is null)
        {
            return MutationResult<SubCategoryDto>.NotFound();
        }

        var targetCategoryExists = await db.ExpenseCategories.AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId, cancellationToken);
        if (!targetCategoryExists)
        {
            return MutationResult<SubCategoryDto>.Fail("Category not found.");
        }

        var nameTaken = await db.ExpenseSubCategories.AnyAsync(
            s => s.ExpenseCategoryId == request.CategoryId && s.Id != subCategoryId && s.Name == request.Name,
            cancellationToken);

        if (nameTaken)
        {
            return MutationResult<SubCategoryDto>.Fail("A subcategory with this name already exists in this category.");
        }

        if (subCategory.ExpenseCategoryId != request.CategoryId)
        {
            // Moving to a new parent: append at the end of its subcategory list
            // rather than keeping the old DisplayOrder, which was only ever
            // meaningful relative to the old parent's other subcategories.
            var nextDisplayOrder = 1 + (await db.ExpenseSubCategories
                .Where(s => s.ExpenseCategoryId == request.CategoryId)
                .Select(s => (int?)s.DisplayOrder)
                .MaxAsync(cancellationToken) ?? -1);

            subCategory.ExpenseCategoryId = request.CategoryId;
            subCategory.DisplayOrder = nextDisplayOrder;
        }

        subCategory.Name = request.Name;
        subCategory.Icon = request.Icon;
        subCategory.IsArchived = request.IsArchived;
        subCategory.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<SubCategoryDto>.Ok(ToDto(subCategory));
    }

    public async Task<bool> ArchiveSubCategoryAsync(Guid userId, Guid subCategoryId, CancellationToken cancellationToken = default)
    {
        var subCategory = await db.ExpenseSubCategories.FirstOrDefaultAsync(s => s.Id == subCategoryId && s.UserId == userId, cancellationToken);
        if (subCategory is null)
        {
            return false;
        }

        subCategory.IsArchived = true;
        subCategory.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ReorderSubCategoriesAsync(Guid userId, Guid categoryId, ReorderRequest request, CancellationToken cancellationToken = default)
    {
        var subCategories = await db.ExpenseSubCategories
            .Where(s => s.UserId == userId && s.ExpenseCategoryId == categoryId && request.OrderedIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (subCategories.Count != request.OrderedIds.Count)
        {
            return false;
        }

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            var subCategory = subCategories.First(s => s.Id == request.OrderedIds[i]);
            subCategory.DisplayOrder = i;
            subCategory.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static SubCategoryDto ToDto(ExpenseSubCategory s) => new(s.Id, s.Name, s.Icon, s.DisplayOrder, s.IsArchived);
}
