using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Transactions.Dtos;

namespace PersonalExpenseManager.Application.Dashboard.Dtos;

public record DashboardDto(
    decimal TotalBalance,
    IReadOnlyList<AccountTypeBalanceDto> BalanceByAccountType,
    decimal ThisMonthIncome,
    decimal ThisMonthExpense,
    decimal NetCashFlow,
    decimal? SavingsRatePercent,
    decimal? ExpenseGrowthPercent,
    IReadOnlyList<CategoryAmountDto> TopExpenseCategories,
    IReadOnlyList<MonthlyAmountDto> IncomeVsExpenseTrend,
    IReadOnlyList<TransactionDto> RecentTransactions,
    IReadOnlyList<AccountDto> Accounts,
    IReadOnlyList<string> Insights);
