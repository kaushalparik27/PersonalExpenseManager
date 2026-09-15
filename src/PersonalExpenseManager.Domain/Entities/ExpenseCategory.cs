using PersonalExpenseManager.Domain.Common;

namespace PersonalExpenseManager.Domain.Entities;

public class ExpenseCategory : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ExpenseSubCategory> SubCategories { get; set; } = [];
}
