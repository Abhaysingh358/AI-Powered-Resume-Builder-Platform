using ResumeAI.AI.Entities;
using ResumeAI.AI.Enums;

namespace ResumeAI.AI.Repositories.Interfaces;

public interface IAiRequestRepository
{
    Task<IList<AiRequest>> FindByUserIdAsync(int userId, CancellationToken ct = default);
    Task<IList<AiRequest>> FindByResumeIdAsync(int resumeId, CancellationToken ct = default);
    Task<AiRequest?>       FindByRequestIdAsync(string requestId, CancellationToken ct = default);
    Task<IList<AiRequest>> FindByRequestTypeAsync(RequestType requestType, CancellationToken ct = default);
    Task<IList<AiRequest>> FindByStatusAsync(RequestStatus status, CancellationToken ct = default);

    // Count AI calls made by a user in the current calendar month
    Task<int> CountByUserIdThisMonthAsync(int userId, RequestType requestType, CancellationToken ct = default);

    // Sum of tokens used by a user — for admin cost monitoring
    Task<int> SumTokensByUserIdAsync(int userId, CancellationToken ct = default);

    Task<AiRequest> CreateAsync(AiRequest request, CancellationToken ct = default);
    Task<AiRequest> UpdateAsync(AiRequest request, CancellationToken ct = default);
}
