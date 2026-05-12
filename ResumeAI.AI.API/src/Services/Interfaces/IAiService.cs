using ResumeAI.AI.DTOs.Request;
using ResumeAI.AI.DTOs.Response;
using ResumeAI.AI.Entities;

namespace ResumeAI.AI.Services.Interfaces;

public interface IAiService
{
    Task<string>              GenerateSummaryAsync(int userId, string subscriptionPlan, GenerateSummaryRequest request, CancellationToken ct = default);
    Task<IList<string>>       GenerateBulletPointsAsync(int userId, string subscriptionPlan, GenerateBulletsRequest request, CancellationToken ct = default);
    Task<string>              GenerateCoverLetterAsync(int userId, string subscriptionPlan, GenerateCoverLetterRequest request, CancellationToken ct = default);
    Task<string>              ImproveSectionAsync(int userId, string subscriptionPlan, ImproveSectionRequest request, CancellationToken ct = default);
    Task<AtsReportResponse>   CheckAtsCompatibilityAsync(int userId, string subscriptionPlan, CheckAtsRequest request, CancellationToken ct = default);
    Task<IList<string>>       SuggestSkillsAsync(int userId, string subscriptionPlan, SuggestSkillsRequest request, CancellationToken ct = default);
    Task<string>              TailorResumeForJobAsync(int userId, string subscriptionPlan, TailorResumeRequest request, CancellationToken ct = default);
    Task<string>              TranslateResumeAsync(int userId, string subscriptionPlan, TranslateResumeRequest request, CancellationToken ct = default);
    Task<IList<AiRequest>>    GetAiHistoryAsync(int userId, CancellationToken ct = default);
    Task<QuotaResponse>       GetRemainingQuotaAsync(int userId, string subscriptionPlan, CancellationToken ct = default);
}
