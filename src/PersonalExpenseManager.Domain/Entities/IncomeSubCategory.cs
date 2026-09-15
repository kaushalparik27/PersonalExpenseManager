using PersonalExpenseManager.Domain.Common;

namespace PersonalExpenseManager.Domain.Entities;

public class IncomeSubCategory : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public Guid IncomeCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    public IncomeCategory? IncomeCategory { get; set; }
}
