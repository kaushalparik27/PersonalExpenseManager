using PersonalExpenseManager.Application.Dashboard.Dtos;

namespace PersonalExpenseManager.Application.Dashboard;

public interface IDashboardQueries
{
    Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BalanceTrendPointDto>> GetBalanceTrendAsync(
        Guid userId, BalanceTrendGranularity granularity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PeriodSummaryDto>> GetPeriodSummariesAsync(Guid userId, CancellationToken cancellationToken = default);
}
