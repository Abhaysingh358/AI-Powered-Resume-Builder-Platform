using ResumeAI.Export.Services.Interfaces;

namespace ResumeAI.Export.BackgroundServices;

// Runs daily and deletes export job records where ExpiresAt < DateTime.UtcNow
public class ExportCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExportCleanupBackgroundService> _logger;

    public ExportCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExportCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Export cleanup background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();
                await exportService.CleanupExpiredExportsAsync(stoppingToken);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during export cleanup.");
            }

            // Run once every 24 hours
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
