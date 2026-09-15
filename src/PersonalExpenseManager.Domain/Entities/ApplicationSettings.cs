namespace PersonalExpenseManager.Domain.Entities;

// One row per user (UserId is both the primary key and the ownership key).
public class ApplicationSettings
{
    public Guid UserId { get; set; }
    public string DefaultCurrency { get; set; } = "INR";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
