namespace PersonalExpenseManager.Application.Transactions.Dtos;

public record TransactionListQuery(
    int Page = 1,
    int PageSize = 25,
    string SortBy = "transactionDate",
    string SortDirection = "desc",
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    Guid? SubCategoryId = null,
    string? TransactionType = null,
    string? Search = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null);
