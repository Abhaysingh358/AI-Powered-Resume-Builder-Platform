using ResumeAI.Resume.DTOs.Request;
using ResumeAI.Resume.DTOs.Response;
using ResumeAI.Resume.Entities;

namespace ResumeAI.Resume.Services.Interfaces;

public interface IResumeService
{
    /// <summary>Create a new resume. Enforces FREE tier limit of 3 resumes.</summary>
    Task<ResumeResponse> CreateResumeAsync(int userId, string subscriptionPlan, CreateResumeRequest request, CancellationToken ct = default);

    /// <summary>Get a single resume by ID. Increments ViewCount if IsPublic.</summary>
    Task<ResumeResponse> GetResumeByIdAsync(int resumeId, int requestingUserId, CancellationToken ct = default);

    /// <summary>Get all resumes belonging to a user.</summary>
    Task<IList<ResumeResponse>> GetResumesByUserAsync(int userId, CancellationToken ct = default);

    /// <summary>Update resume fields. Only owner can update.</summary>
    Task<ResumeResponse> UpdateResumeAsync(int resumeId, int userId, UpdateResumeRequest request, CancellationToken ct = default);

    /// <summary>Permanently delete a resume. Only owner can delete.</summary>
    Task DeleteResumeAsync(int resumeId, int userId, CancellationToken ct = default);

    /// <summary>Deep copy a resume and all its sections. Enforces FREE tier limit.</summary>
    Task<ResumeResponse> DuplicateResumeAsync(int resumeId, int userId, string subscriptionPlan, CancellationToken ct = default);

    /// <summary>Atomically update ATS score — called by AI Service.</summary>
    Task UpdateAtsScoreAsync(int resumeId, int userId, int atsScore, CancellationToken ct = default);

    /// <summary>Set IsPublic = true to share to public gallery.</summary>
    Task PublishResumeAsync(int resumeId, int userId, CancellationToken ct = default);

    /// <summary>Set IsPublic = false to remove from public gallery.</summary>
    Task UnpublishResumeAsync(int resumeId, int userId, CancellationToken ct = default);

    /// <summary>Get all publicly shared resumes for the gallery.</summary>
    Task<IList<ResumeResponse>> GetPublicResumesAsync(CancellationToken ct = default);

    /// <summary>Get all resumes using a specific template.</summary>
    Task<IList<ResumeResponse>> GetResumesByTemplateAsync(int templateId, CancellationToken ct = default);
}
