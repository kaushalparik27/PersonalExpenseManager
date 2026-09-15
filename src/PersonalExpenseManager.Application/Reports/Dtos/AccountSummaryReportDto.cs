namespace PersonalExpenseManager.Application.Reports.Dtos;

public record AccountSummaryItemDto(
    Guid AccountId,
    string AccountName,
    string AccountType,
    decimal PeriodIncome,
    decimal PeriodExpense,
    decimal PeriodTransferIn,
    decimal PeriodTransferOut,
    decimal PeriodNetChange,
    decimal CurrentBalance);

public record AccountSummaryReportDto(IReadOnlyList<AccountSummaryItemDto> Accounts);
