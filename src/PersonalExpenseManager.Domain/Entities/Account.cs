using PersonalExpenseManager.Domain.Common;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public string? Description { get; set; }
    public decimal OpeningBalance { get; set; }
    public string Currency { get; set; } = "INR";
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? Icon { get; set; }
}
