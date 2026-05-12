using AutoMapper;
using ResumeAI.Resume.DTOs.Request;
using ResumeAI.Resume.DTOs.Response;
using ResumeAI.Resume.Entities;
using ResumeAI.Resume.Enums;
using ResumeAI.Resume.Repositories.Interfaces;
using ResumeAI.Resume.Services.Interfaces;

namespace ResumeAI.Resume.Services;

public class ResumeService : IResumeService
{
    private readonly IResumeRepository       _resumeRepo;
    private readonly IMapper                 _mapper;
    private readonly ILogger<ResumeService>  _logger;

    // FREE tier max resume count enforced here
    private const int FreeResumeLimit = 3;

    public ResumeService(
        IResumeRepository resumeRepo,
        IMapper mapper,
        ILogger<ResumeService> logger)
    {
        _resumeRepo = resumeRepo;
        _mapper     = mapper;
        _logger     = logger;
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<ResumeResponse> CreateResumeAsync(
        int userId, string subscriptionPlan,
        CreateResumeRequest request, CancellationToken ct = default)
    {
        // Enforce FREE tier limit
        await EnforceFreeUserLimitAsync(userId, subscriptionPlan, ct);

        var resume = new ResumeEntity
        {
            UserId         = userId,
            Title          = request.Title.Trim(),
            TargetJobTitle = request.TargetJobTitle?.Trim(),
            TemplateId     = request.TemplateId,
            Language       = request.Language,
            Status         = ResumeStatus.DRAFT,
            IsPublic       = false,
            AtsScore       = 0,
            ViewCount      = 0
        };

        var created = await _resumeRepo.CreateAsync(resume, ct);
        _logger.LogInformation("Resume {ResumeId} created by user {UserId}", created.ResumeId, userId);
        return _mapper.Map<ResumeResponse>(created);
    }

    // ── Get By ID ─────────────────────────────────────────────────────────────
    public async Task<ResumeResponse> GetResumeByIdAsync(
        int resumeId, int requestingUserId, CancellationToken ct = default)
    {
        var resume = await RequireResumeAsync(resumeId, ct);

        // If public — increment view count for non-owners
        if (resume.IsPublic && resume.UserId != requestingUserId)
            await _resumeRepo.IncrementViewCountAsync(resumeId, ct);

        // Private resume — only owner can view
        if (!resume.IsPublic && resume.UserId != requestingUserId)
            throw new UnauthorizedAccessException("You do not have access to this resume.");

        return _mapper.Map<ResumeResponse>(resume);
    }

    // ── Get By User ───────────────────────────────────────────────────────────
    public async Task<IList<ResumeResponse>> GetResumesByUserAsync(int userId, CancellationToken ct = default)
    {
        var resumes = await _resumeRepo.FindByUserIdAsync(userId, ct);
        return _mapper.Map<IList<ResumeResponse>>(resumes);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public async Task<ResumeResponse> UpdateResumeAsync(
        int resumeId, int userId,
        UpdateResumeRequest request, CancellationToken ct = default)
    {
        var resume = await RequireOwnerAsync(resumeId, userId, ct);

        if (!string.IsNullOrWhiteSpace(request.Title))
            resume.Title = request.Title.Trim();

        if (request.TargetJobTitle is not null)
            resume.TargetJobTitle = request.TargetJobTitle.Trim();

        if (request.TemplateId.HasValue)
            resume.TemplateId = request.TemplateId.Value;

        if (!string.IsNullOrWhiteSpace(request.Language))
            resume.Language = request.Language;

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ResumeStatus>(request.Status, ignoreCase: true, out var status))
            resume.Status = status;

        var updated = await _resumeRepo.UpdateAsync(resume, ct);
        _logger.LogInformation("Resume {ResumeId} updated by user {UserId}", resumeId, userId);
        return _mapper.Map<ResumeResponse>(updated);
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    public async Task DeleteResumeAsync(int resumeId, int userId, CancellationToken ct = default)
    {
        await RequireOwnerAsync(resumeId, userId, ct);
        await _resumeRepo.DeleteByResumeIdAsync(resumeId, ct);
        _logger.LogInformation("Resume {ResumeId} deleted by user {UserId}", resumeId, userId);
    }

    // ── Duplicate (deep copy using no-tracking pattern) ───────────────────────
    public async Task<ResumeResponse> DuplicateResumeAsync(
        int resumeId, int userId, string subscriptionPlan, CancellationToken ct = default)
    {
        // Enforce FREE tier limit before duplicating
        await EnforceFreeUserLimitAsync(userId, subscriptionPlan, ct);

        var original = await RequireOwnerAsync(resumeId, userId, ct);

        // Deep copy — create new entity with same data, new ID
        var copy = new ResumeEntity
        {
            UserId         = original.UserId,
            Title          = $"{original.Title} (Copy)",
            TargetJobTitle = original.TargetJobTitle,
            TemplateId     = original.TemplateId,
            Language       = original.Language,
            Status         = ResumeStatus.DRAFT,   // copies start as DRAFT
            IsPublic       = false,                 // copies are private by default
            AtsScore       = 0,                     // reset ATS score for copy
            ViewCount      = 0                      // reset view count
        };

        var created = await _resumeRepo.CreateAsync(copy, ct);
        _logger.LogInformation("Resume {OriginalId} duplicated to {NewId} by user {UserId}",
            resumeId, created.ResumeId, userId);

        return _mapper.Map<ResumeResponse>(created);
    }

    // ── ATS Score (atomic — called by AI Service) ─────────────────────────────
    public async Task UpdateAtsScoreAsync(
        int resumeId, int userId, int atsScore, CancellationToken ct = default)
    {
        await RequireOwnerAsync(resumeId, userId, ct);
        await _resumeRepo.UpdateAtsScoreAsync(resumeId, atsScore, ct);
        _logger.LogInformation("ATS score for Resume {ResumeId} updated to {Score}", resumeId, atsScore);
    }

    // ── Publish / Unpublish ───────────────────────────────────────────────────
    public async Task PublishResumeAsync(int resumeId, int userId, CancellationToken ct = default)
    {
        await RequireOwnerAsync(resumeId, userId, ct);
        await _resumeRepo.PublishAsync(resumeId, ct);
        _logger.LogInformation("Resume {ResumeId} published by user {UserId}", resumeId, userId);
    }

    public async Task UnpublishResumeAsync(int resumeId, int userId, CancellationToken ct = default)
    {
        await RequireOwnerAsync(resumeId, userId, ct);
        await _resumeRepo.UnpublishAsync(resumeId, ct);
        _logger.LogInformation("Resume {ResumeId} unpublished by user {UserId}", resumeId, userId);
    }

    // ── Public Gallery ────────────────────────────────────────────────────────
    public async Task<IList<ResumeResponse>> GetPublicResumesAsync(CancellationToken ct = default)
    {
        var resumes = await _resumeRepo.FindByIsPublicAsync(true, ct);
        return _mapper.Map<IList<ResumeResponse>>(resumes);
    }

    // ── By Template ───────────────────────────────────────────────────────────
    public async Task<IList<ResumeResponse>> GetResumesByTemplateAsync(int templateId, CancellationToken ct = default)
    {
        var resumes = await _resumeRepo.FindByTemplateIdAsync(templateId, ct);
        return _mapper.Map<IList<ResumeResponse>>(resumes);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Loads a resume or throws 404.</summary>
    private async Task<ResumeEntity> RequireResumeAsync(int resumeId, CancellationToken ct)
        => await _resumeRepo.FindByResumeIdAsync(resumeId, ct)
           ?? throw new KeyNotFoundException($"Resume {resumeId} not found.");

    /// <summary>Loads a resume and verifies the requesting user is the owner.</summary>
    private async Task<ResumeEntity> RequireOwnerAsync(int resumeId, int userId, CancellationToken ct)
    {
        var resume = await RequireResumeAsync(resumeId, ct);
        if (resume.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to modify this resume.");
        return resume;
    }

    /// <summary>Throws if FREE user already has 3 or more resumes.</summary>
    private async Task EnforceFreeUserLimitAsync(int userId, string subscriptionPlan, CancellationToken ct)
    {
        if (subscriptionPlan.Equals("PREMIUM", StringComparison.OrdinalIgnoreCase))
            return; // Premium users have no limit

        var count = await _resumeRepo.CountByUserIdAsync(userId, ct);
        if (count >= FreeResumeLimit)
            throw new InvalidOperationException(
                $"Free plan users can create a maximum of {FreeResumeLimit} resumes. " +
                "Upgrade to Premium for unlimited resumes.");
    }
}
