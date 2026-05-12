using Microsoft.EntityFrameworkCore;
using ResumeAI.Export.Data;
using ResumeAI.Export.Entities;
using ResumeAI.Export.Enums;
using ResumeAI.Export.Repositories.Interfaces;

namespace ResumeAI.Export.Repositories;

public class ExportRepository : IExportRepository
{
    private readonly ExportDbContext _db;

    public ExportRepository(ExportDbContext db) => _db = db;

    public async Task<ExportJob?> FindByJobIdAsync(string jobId, CancellationToken ct = default)
        => await _db.ExportJobs.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.JobId == jobId, ct);

    public async Task<IList<ExportJob>> FindByUserIdAsync(int userId, CancellationToken ct = default)
        => await _db.ExportJobs.AsNoTracking()
                    .Where(e => e.UserId == userId)
                    .OrderByDescending(e => e.RequestedAt)
                    .ToListAsync(ct);

    public async Task<IList<ExportJob>> FindByResumeIdAsync(int resumeId, CancellationToken ct = default)
        => await _db.ExportJobs.AsNoTracking()
                    .Where(e => e.ResumeId == resumeId)
                    .OrderByDescending(e => e.RequestedAt)
                    .ToListAsync(ct);

    public async Task<IList<ExportJob>> FindByStatusAsync(ExportStatus status, CancellationToken ct = default)
        => await _db.ExportJobs.AsNoTracking()
                    .Where(e => e.Status == status)
                    .ToListAsync(ct);

    public async Task<IList<ExportJob>> FindByFormatAsync(ExportFormat format, CancellationToken ct = default)
        => await _db.ExportJobs.AsNoTracking()
                    .Where(e => e.Format == format)
                    .ToListAsync(ct);

    public async Task<IList<ExportJob>> FindExpiredJobsAsync(DateTime cutoff, CancellationToken ct = default)
        => await _db.ExportJobs.AsNoTracking()
                    .Where(e => e.ExpiresAt < cutoff)
                    .ToListAsync(ct);

    public async Task<int> CountByUserIdTodayAsync(int userId, CancellationToken ct = default)
    {
        var startOfDay = DateTime.UtcNow.Date;
        return await _db.ExportJobs
                        .CountAsync(e => e.UserId == userId && e.RequestedAt >= startOfDay, ct);
    }

    public async Task<int> CountPdfByUserIdTodayAsync(int userId, CancellationToken ct = default)
    {
        var startOfDay = DateTime.UtcNow.Date;
        return await _db.ExportJobs
                        .CountAsync(e => e.UserId == userId &&
                                        e.Format == ExportFormat.PDF &&
                                        e.Status != ExportStatus.FAILED &&
                                        e.RequestedAt >= startOfDay, ct);
    }

    public async Task<ExportJob> CreateAsync(ExportJob job, CancellationToken ct = default)
    {
        job.RequestedAt = DateTime.UtcNow;
        job.ExpiresAt   = DateTime.UtcNow.AddDays(7);
        _db.ExportJobs.Add(job);
        await _db.SaveChangesAsync(ct);
        return job;
    }

    public async Task<ExportJob> UpdateAsync(ExportJob job, CancellationToken ct = default)
    {
        _db.ExportJobs.Update(job);
        await _db.SaveChangesAsync(ct);
        return job;
    }

    public async Task DeleteByJobIdAsync(string jobId, CancellationToken ct = default)
        => await _db.ExportJobs.Where(e => e.JobId == jobId).ExecuteDeleteAsync(ct);

    public async Task DeleteExpiredAsync(CancellationToken ct = default)
        => await _db.ExportJobs
                    .Where(e => e.ExpiresAt < DateTime.UtcNow)
                    .ExecuteDeleteAsync(ct);
}
