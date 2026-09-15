using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Categories;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Persistence.Queries;

namespace PersonalExpenseManager.Infrastructure.Persistence.Commands;

public class IncomeCategoryCommands(ApplicationDbContext db) : IIncomeCategoryCommands
{
    public async Task<MutationResult<CategoryDto>> CreateCategoryAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var nameTaken = await db.IncomeCategories.AnyAsync(c => c.UserId == userId && c.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            return MutationResult<CategoryDto>.Fail("A category with this name already exists.");
        }

        var nextDisplayOrder = 1 + (await db.IncomeCategories
            .Where(c => c.UserId == userId)
            .Select(c => (int?)c.DisplayOrder)
            .MaxAsync(cancellationToken) ?? -1);

        var category = new IncomeCategory
        {
            UserId = userId,
            Name = request.Name,
            Icon = request.Icon,
            DisplayOrder = nextDisplayOrder,
        };

        db.IncomeCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<CategoryDto>.Ok(IncomeCategoryQueries.ToDto(category));
    }

    public async Task<MutationResult<CategoryDto>> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await db.IncomeCategories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);

        if (category is null)
        {
            return MutationResult<CategoryDto>.NotFound();
        }

        var nameTaken = await db.IncomeCategories
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

        return MutationResult<CategoryDto>.Ok(IncomeCategoryQueries.ToDto(category));
    }

    public async Task<bool> ArchiveCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await db.IncomeCategories.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);
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
        var categories = await db.IncomeCategories
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
        var categoryExists = await db.IncomeCategories.AnyAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);
        if (!categoryExists)
        {
            return MutationResult<SubCategoryDto>.NotFound();
        }

        var nameTaken = await db.IncomeSubCategories
            .AnyAsync(s => s.IncomeCategoryId == categoryId && s.Name == request.Name, cancellationToken);

        if (nameTaken)
        {
            return MutationResult<SubCategoryDto>.Fail("A subcategory with this name already exists in this category.");
        }

        var nextDisplayOrder = 1 + (await db.IncomeSubCategories
            .Where(s => s.IncomeCategoryId == categoryId)
            .Select(s => (int?)s.DisplayOrder)
            .MaxAsync(cancellationToken) ?? -1);

        var subCategory = new IncomeSubCategory
        {
            UserId = userId,
            IncomeCategoryId = categoryId,
            Name = request.Name,
            DisplayOrder = nextDisplayOrder,
        };

        db.IncomeSubCategories.Add(subCategory);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<SubCategoryDto>.Ok(ToDto(subCategory));
    }

    public async Task<MutationResult<SubCategoryDto>> UpdateSubCategoryAsync(Guid userId, Guid subCategoryId, UpdateSubCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var subCategory = await db.IncomeSubCategories.FirstOrDefaultAsync(s => s.Id == subCategoryId && s.UserId == userId, cancellationToken);
        if (subCategory is null)
        {
            return MutationResult<SubCategoryDto>.NotFound();
        }

        var nameTaken = await db.IncomeSubCategories.AnyAsync(
            s => s.IncomeCategoryId == subCategory.IncomeCategoryId && s.Id != subCategoryId && s.Name == request.Name,
            cancellationToken);

        if (nameTaken)
        {
            return MutationResult<SubCategoryDto>.Fail("A subcategory with this name already exists in this category.");
        }

        subCategory.Name = request.Name;
        subCategory.IsArchived = request.IsArchived;
        subCategory.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<SubCategoryDto>.Ok(ToDto(subCategory));
    }

    public async Task<bool> ArchiveSubCategoryAsync(Guid userId, Guid subCategoryId, CancellationToken cancellationToken = default)
    {
        var subCategory = await db.IncomeSubCategories.FirstOrDefaultAsync(s => s.Id == subCategoryId && s.UserId == userId, cancellationToken);
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
        var subCategories = await db.IncomeSubCategories
            .Where(s => s.UserId == userId && s.IncomeCategoryId == categoryId && request.OrderedIds.Contains(s.Id))
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

    private static SubCategoryDto ToDto(IncomeSubCategory s) => new(s.Id, s.Name, s.DisplayOrder, s.IsArchived);
}
