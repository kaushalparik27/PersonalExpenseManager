using PersonalExpenseManager.Domain.Common;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Domain.Entities;

public class RecurringTransaction : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public TransactionType TransactionType { get; set; }

    public Guid AccountId { get; set; }
    public Guid? TransferToAccountId { get; set; }

    public decimal Amount { get; set; }

    public Guid? ExpenseCategoryId { get; set; }
    public Guid? ExpenseSubCategoryId { get; set; }
    public Guid? IncomeCategoryId { get; set; }
    public Guid? IncomeSubCategoryId { get; set; }

    public string? Description { get; set; }

    public RecurrenceFrequency Frequency { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly NextOccurrenceDate { get; set; }
    public bool IsActive { get; set; } = true;
}
