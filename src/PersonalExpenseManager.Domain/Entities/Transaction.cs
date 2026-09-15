using PersonalExpenseManager.Domain.Common;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Domain.Entities;

public class Transaction : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public DateOnly TransactionDate { get; set; }
    public TransactionType TransactionType { get; set; }

    public Guid AccountId { get; set; }

    // Set only when TransactionType is Transfer: the destination account.
    public Guid? TransferToAccountId { get; set; }

    public decimal Amount { get; set; }

    // Exactly one of the Expense*/Income* pairs is set, matching TransactionType;
    // both are null for Transfer and Adjustment transactions.
    public Guid? ExpenseCategoryId { get; set; }
    public Guid? ExpenseSubCategoryId { get; set; }
    public Guid? IncomeCategoryId { get; set; }
    public Guid? IncomeSubCategoryId { get; set; }

    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Payee { get; set; }
    public string? ReferenceNumber { get; set; }

    public bool IsRecurring { get; set; }
    public Guid? RecurringTransactionId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<TransactionTag> TransactionTags { get; set; } = [];
}
