using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalExpenseManager.Application.RecurringTransactions;

namespace PersonalExpenseManager.Infrastructure.Services;

// Runs the recurring transaction engine (RecurringTransactionProcessor) on a timer,
// in-process — a PeriodicTimer BackgroundService is the standard ASP.NET Core way to
// do this without pulling in a message broker or external scheduler (explicitly out
// of scope per the spec). Not registered in the Testing environment (see Program.cs)
// so integration tests control exactly when the engine runs.
public class RecurringTransactionProcessingService(IServiceScopeFactory scopeFactory, ILogger<RecurringTransactionProcessingService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Catch up anything due immediately on startup (e.g. the app was offline
        // when a recurring transaction's next occurrence came due), then settle
        // into the regular interval.
        await ProcessSafelyAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessSafelyAsync(stoppingToken);
        }
    }

    private async Task ProcessSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IRecurringTransactionProcessor>();
            await processor.ProcessDueAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Recurring transaction engine run failed; will retry on the next interval.");
        }
    }
}
