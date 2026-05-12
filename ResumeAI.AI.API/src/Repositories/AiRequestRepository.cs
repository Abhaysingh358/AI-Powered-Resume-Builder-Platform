using Microsoft.EntityFrameworkCore;
using ResumeAI.AI.Data;
using ResumeAI.AI.Entities;
using ResumeAI.AI.Enums;
using ResumeAI.AI.Repositories.Interfaces;

namespace ResumeAI.AI.Repositories;

public class AiRequestRepository : IAiRequestRepository
{
    private readonly AiDbContext _db;

    public AiRequestRepository(AiDbContext db) => _db = db;

    public async Task<IList<AiRequest>> FindByUserIdAsync(int userId, CancellationToken ct = default)
        => await _db.AiRequests
                    .AsNoTracking()
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync(ct);

    public async Task<IList<AiRequest>> FindByResumeIdAsync(int resumeId, CancellationToken ct = default)
        => await _db.AiRequests
                    .AsNoTracking()
                    .Where(a => a.ResumeId == resumeId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync(ct);

    public async Task<AiRequest?> FindByRequestIdAsync(string requestId, CancellationToken ct = default)
        => await _db.AiRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.RequestId == requestId, ct);

    public async Task<IList<AiRequest>> FindByRequestTypeAsync(RequestType requestType, CancellationToken ct = default)
        => await _db.AiRequests
                    .AsNoTracking()
                    .Where(a => a.RequestType == requestType)
                    .ToListAsync(ct);

    public async Task<IList<AiRequest>> FindByStatusAsync(RequestStatus status, CancellationToken ct = default)
        => await _db.AiRequests
                    .AsNoTracking()
                    .Where(a => a.Status == status)
                    .ToListAsync(ct);

    public async Task<int> CountByUserIdThisMonthAsync(int userId, RequestType requestType, CancellationToken ct = default)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return await _db.AiRequests
                        .CountAsync(a => a.UserId == userId &&
                                        a.RequestType == requestType &&
                                        a.Status != RequestStatus.FAILED &&
                                        a.CreatedAt >= startOfMonth, ct);
    }

    public async Task<int> SumTokensByUserIdAsync(int userId, CancellationToken ct = default)
        => await _db.AiRequests
                    .Where(a => a.UserId == userId)
                    .SumAsync(a => a.TokensUsed, ct);

    public async Task<AiRequest> CreateAsync(AiRequest request, CancellationToken ct = default)
    {
        request.CreatedAt = DateTime.UtcNow;
        _db.AiRequests.Add(request);
        await _db.SaveChangesAsync(ct);
        return request;
    }

    public async Task<AiRequest> UpdateAsync(AiRequest request, CancellationToken ct = default)
    {
        _db.AiRequests.Update(request);
        await _db.SaveChangesAsync(ct);
        return request;
    }
}
