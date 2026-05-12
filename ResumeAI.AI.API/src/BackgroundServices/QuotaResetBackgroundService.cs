using ResumeAI.AI.Services;

namespace ResumeAI.AI.BackgroundServices;

// Runs continuously in the background.
// Checks every hour if it is the 1st day of a new month.
// When it is, triggers the quota reset.
// Redis keys expire automatically via TTL, but this service
// provides an explicit reset as a safety net.
public class QuotaResetBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QuotaResetBackgroundService> _logger;
    private DateTime _lastResetDate;

    public QuotaResetBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<QuotaResetBackgroundService> logger)
    {
        _scopeFactory  = scopeFactory;
        _logger        = logger;
        _lastResetDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Quota reset background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now           = DateTime.UtcNow;
            var currentMonth  = new DateTime(now.Year, now.Month, 1);

            // If we are in a new month that hasn't been reset yet
            if (currentMonth > _lastResetDate)
            {
                _logger.LogInformation("New month detected. Running monthly quota reset for {Month}/{Year}", now.Month, now.Year);

                using var scope        = _scopeFactory.CreateScope();
                var quotaService       = scope.ServiceProvider.GetRequiredService<IQuotaService>();
                await quotaService.ResetAllAsync(stoppingToken);

                _lastResetDate = currentMonth;
                _logger.LogInformation("Monthly quota reset completed.");
            }

            // Check every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
