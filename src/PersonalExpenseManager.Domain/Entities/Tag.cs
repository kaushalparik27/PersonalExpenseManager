using PersonalExpenseManager.Domain.Common;

namespace PersonalExpenseManager.Domain.Entities;

public class Tag : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}
