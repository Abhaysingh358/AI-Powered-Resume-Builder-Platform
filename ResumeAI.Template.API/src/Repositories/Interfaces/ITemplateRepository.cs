using ResumeAI.Template.Entities;
using ResumeAI.Template.Enums;

namespace ResumeAI.Template.Repositories.Interfaces;

public interface ITemplateRepository
{
    Task<ResumeTemplate?>       FindByTemplateIdAsync(int templateId, CancellationToken ct = default);
    Task<IList<ResumeTemplate>> FindByCategoryAsync(TemplateCategory category, CancellationToken ct = default);
    Task<IList<ResumeTemplate>> FindByIsPremiumAsync(bool isPremium, CancellationToken ct = default);
    Task<IList<ResumeTemplate>> FindByIsActiveAsync(bool isActive, CancellationToken ct = default);

    // Returns all active templates ordered by UsageCount descending
    Task<IList<ResumeTemplate>> FindAllOrderByUsageCountDescAsync(CancellationToken ct = default);

    Task<int> CountByCategoryAsync(TemplateCategory category, CancellationToken ct = default);

    Task<ResumeTemplate> CreateAsync(ResumeTemplate template, CancellationToken ct = default);
    Task<ResumeTemplate> UpdateAsync(ResumeTemplate template, CancellationToken ct = default);

    // Atomic increment — no full entity load
    Task UpdateUsageCountAsync(int templateId, CancellationToken ct = default);
}
