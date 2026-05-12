using ResumeAI.Resume.Entities;
using ResumeAI.Resume.Enums;

namespace ResumeAI.Resume.Repositories.Interfaces;

public interface IResumeRepository
{
    // ── Queries ───────────────────────────────────────────────────────────────
    Task<ResumeEntity?>       FindByResumeIdAsync(int resumeId, CancellationToken ct = default);
    Task<IList<ResumeEntity>> FindByUserIdAsync(int userId, CancellationToken ct = default);
    Task<IList<ResumeEntity>> FindByStatusAsync(ResumeStatus status, CancellationToken ct = default);
    Task<IList<ResumeEntity>> FindByTargetJobTitleAsync(string jobTitle, CancellationToken ct = default);
    Task<IList<ResumeEntity>> FindByIsPublicAsync(bool isPublic, CancellationToken ct = default);
    Task<IList<ResumeEntity>> FindByTemplateIdAsync(int templateId, CancellationToken ct = default);
    Task<int>                 CountByUserIdAsync(int userId, CancellationToken ct = default);

    // ── Commands ──────────────────────────────────────────────────────────────
    Task<ResumeEntity> CreateAsync(ResumeEntity resume, CancellationToken ct = default);
    Task<ResumeEntity> UpdateAsync(ResumeEntity resume, CancellationToken ct = default);
    Task               DeleteByResumeIdAsync(int resumeId, CancellationToken ct = default);

    // ── Atomic Updates (no full entity load) ──────────────────────────────────
    Task UpdateAtsScoreAsync(int resumeId, int atsScore, CancellationToken ct = default);
    Task PublishAsync(int resumeId, CancellationToken ct = default);
    Task UnpublishAsync(int resumeId, CancellationToken ct = default);
    Task IncrementViewCountAsync(int resumeId, CancellationToken ct = default);
}
