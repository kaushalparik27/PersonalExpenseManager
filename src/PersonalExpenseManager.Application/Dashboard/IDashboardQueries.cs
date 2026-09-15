using PersonalExpenseManager.Application.Dashboard.Dtos;

namespace PersonalExpenseManager.Application.Dashboard;

public interface IDashboardQueries
{
    Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
}
