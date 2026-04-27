using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ResumeAI.AI.Configuration;
using ResumeAI.AI.Enums;

namespace ResumeAI.AI.Services;

public interface IQuotaService
{
    Task<int>  GetUsedCountAsync(int userId, RequestType type, CancellationToken ct = default);
    Task       IncrementAsync(int userId, RequestType type, CancellationToken ct = default);
    Task       ResetAllAsync(CancellationToken ct = default);
    bool       IsAtsRequest(RequestType type);
}

public class QuotaService : IQuotaService
{
    private readonly IDistributedCache _cache;
    private readonly AiSettings        _settings;
    private readonly ILogger<QuotaService> _logger;

    public QuotaService(IDistributedCache cache, IOptions<AiSettings> settings, ILogger<QuotaService> logger)
    {
        _cache    = cache;
        _settings = settings.Value;
        _logger   = logger;
    }

    // Redis key format: ai-quota:{userId}:{year}-{month}:{type}
    private static string BuildKey(int userId, RequestType type)
    {
        var now = DateTime.UtcNow;
        return $"ai-quota:{userId}:{now.Year}-{now.Month:D2}:{type}";
    }

    public async Task<int> GetUsedCountAsync(int userId, RequestType type, CancellationToken ct = default)
    {
        var key   = BuildKey(userId, type);
        var value = await _cache.GetStringAsync(key, ct);
        return int.TryParse(value, out var count) ? count : 0;
    }

    public async Task IncrementAsync(int userId, RequestType type, CancellationToken ct = default)
    {
        var key   = BuildKey(userId, type);
        var value = await _cache.GetStringAsync(key, ct);
        var count = int.TryParse(value, out var c) ? c : 0;

        // Expire at end of current month
        var now        = DateTime.UtcNow;
        var nextMonth  = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        var ttl        = nextMonth - now;

        await _cache.SetStringAsync(key, (count + 1).ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);

        _logger.LogInformation("Quota incremented for user {UserId} type {Type}: {Count}", userId, type, count + 1);
    }

    public async Task ResetAllAsync(CancellationToken ct = default)
    {
        // Redis keys expire automatically via TTL set in IncrementAsync
        // This method is called by the BackgroundService on 1st of month as a safety reset
        _logger.LogInformation("Monthly quota reset triggered at {Time}", DateTime.UtcNow);
        await Task.CompletedTask;
    }

    public bool IsAtsRequest(RequestType type) => type == RequestType.ATS;
}
