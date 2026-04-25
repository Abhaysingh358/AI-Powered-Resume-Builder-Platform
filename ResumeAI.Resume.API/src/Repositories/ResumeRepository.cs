using Microsoft.EntityFrameworkCore;
using ResumeAI.Resume.Data;
using ResumeAI.Resume.Entities;
using ResumeAI.Resume.Enums;
using ResumeAI.Resume.Repositories.Interfaces;

namespace ResumeAI.Resume.Repositories;

public class ResumeRepository : IResumeRepository
{
    private readonly ResumeDbContext _db;

    public ResumeRepository(ResumeDbContext db) => _db = db;

    // ── Queries ───────────────────────────────────────────────────────────────
    public async Task<ResumeEntity?> FindByResumeIdAsync(int resumeId, CancellationToken ct = default)
        => await _db.Resumes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ResumeId == resumeId, ct);

    public async Task<IList<ResumeEntity>> FindByUserIdAsync(int userId, CancellationToken ct = default)
        => await _db.Resumes
                    .AsNoTracking()
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.UpdatedAt)
                    .ToListAsync(ct);

    public async Task<IList<ResumeEntity>> FindByStatusAsync(ResumeStatus status, CancellationToken ct = default)
        => await _db.Resumes
                    .AsNoTracking()
                    .Where(r => r.Status == status)
                    .ToListAsync(ct);

    public async Task<IList<ResumeEntity>> FindByTargetJobTitleAsync(string jobTitle, CancellationToken ct = default)
        => await _db.Resumes
                    .AsNoTracking()
                    .Where(r => r.TargetJobTitle != null &&
                                r.TargetJobTitle.ToLower().Contains(jobTitle.ToLower()))
                    .ToListAsync(ct);

    public async Task<IList<ResumeEntity>> FindByIsPublicAsync(bool isPublic, CancellationToken ct = default)
        => await _db.Resumes
                    .AsNoTracking()
                    .Where(r => r.IsPublic == isPublic)
                    .OrderByDescending(r => r.ViewCount)
                    .ToListAsync(ct);

    public async Task<IList<ResumeEntity>> FindByTemplateIdAsync(int templateId, CancellationToken ct = default)
        => await _db.Resumes
                    .AsNoTracking()
                    .Where(r => r.TemplateId == templateId)
                    .ToListAsync(ct);

    public async Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default)
        => await _db.Resumes
                    .CountAsync(r => r.UserId == userId, ct);

    // ── Commands ──────────────────────────────────────────────────────────────
    public async Task<ResumeEntity> CreateAsync(ResumeEntity resume, CancellationToken ct = default)
    {
        resume.CreatedAt = DateTime.UtcNow;
        resume.UpdatedAt = DateTime.UtcNow;
        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync(ct);
        return resume;
    }

    public async Task<ResumeEntity> UpdateAsync(ResumeEntity resume, CancellationToken ct = default)
    {
        resume.UpdatedAt = DateTime.UtcNow;
        _db.Resumes.Update(resume);
        await _db.SaveChangesAsync(ct);
        return resume;
    }

    public async Task DeleteByResumeIdAsync(int resumeId, CancellationToken ct = default)
        => await _db.Resumes
                    .Where(r => r.ResumeId == resumeId)
                    .ExecuteDeleteAsync(ct);

    // ── Atomic Updates using ExecuteUpdateAsync ───────────────────────────────

    /// <summary>Updates only AtsScore column without loading the full entity.</summary>
    public async Task UpdateAtsScoreAsync(int resumeId, int atsScore, CancellationToken ct = default)
        => await _db.Resumes
                    .Where(r => r.ResumeId == resumeId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.AtsScore,   atsScore)
                        .SetProperty(r => r.UpdatedAt,  DateTime.UtcNow), ct);

    /// <summary>Sets IsPublic = true atomically.</summary>
    public async Task PublishAsync(int resumeId, CancellationToken ct = default)
        => await _db.Resumes
                    .Where(r => r.ResumeId == resumeId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.IsPublic,  true)
                        .SetProperty(r => r.UpdatedAt, DateTime.UtcNow), ct);

    /// <summary>Sets IsPublic = false atomically.</summary>
    public async Task UnpublishAsync(int resumeId, CancellationToken ct = default)
        => await _db.Resumes
                    .Where(r => r.ResumeId == resumeId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.IsPublic,  false)
                        .SetProperty(r => r.UpdatedAt, DateTime.UtcNow), ct);

    /// <summary>Increments ViewCount by 1 atomically without loading the entity.</summary>
    public async Task IncrementViewCountAsync(int resumeId, CancellationToken ct = default)
        => await _db.Resumes
                    .Where(r => r.ResumeId == resumeId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.ViewCount, r => r.ViewCount + 1), ct);
}
