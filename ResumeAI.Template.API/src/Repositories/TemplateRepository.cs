using Microsoft.EntityFrameworkCore;
using ResumeAI.Template.Data;
using ResumeAI.Template.Entities;
using ResumeAI.Template.Enums;
using ResumeAI.Template.Repositories.Interfaces;

namespace ResumeAI.Template.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly TemplateDbContext _db;

    public TemplateRepository(TemplateDbContext db) => _db = db;

    public async Task<ResumeTemplate?> FindByTemplateIdAsync(int templateId, CancellationToken ct = default)
        => await _db.ResumeTemplates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TemplateId == templateId, ct);

    public async Task<IList<ResumeTemplate>> FindByCategoryAsync(TemplateCategory category, CancellationToken ct = default)
        => await _db.ResumeTemplates
                    .AsNoTracking()
                    .Where(t => t.Category == category && t.IsActive)
                    .OrderByDescending(t => t.UsageCount)
                    .ToListAsync(ct);

    public async Task<IList<ResumeTemplate>> FindByIsPremiumAsync(bool isPremium, CancellationToken ct = default)
        => await _db.ResumeTemplates
                    .AsNoTracking()
                    .Where(t => t.IsPremium == isPremium && t.IsActive)
                    .OrderByDescending(t => t.UsageCount)
                    .ToListAsync(ct);

    public async Task<IList<ResumeTemplate>> FindByIsActiveAsync(bool isActive, CancellationToken ct = default)
        => await _db.ResumeTemplates
                    .AsNoTracking()
                    .Where(t => t.IsActive == isActive)
                    .OrderByDescending(t => t.UsageCount)
                    .ToListAsync(ct);

    public async Task<IList<ResumeTemplate>> FindAllOrderByUsageCountDescAsync(CancellationToken ct = default)
        => await _db.ResumeTemplates
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .OrderByDescending(t => t.UsageCount)
                    .ToListAsync(ct);

    public async Task<int> CountByCategoryAsync(TemplateCategory category, CancellationToken ct = default)
        => await _db.ResumeTemplates
                    .CountAsync(t => t.Category == category && t.IsActive, ct);

    public async Task<ResumeTemplate> CreateAsync(ResumeTemplate template, CancellationToken ct = default)
    {
        template.CreatedAt  = DateTime.UtcNow;
        template.UsageCount = 0;
        template.IsActive   = true;
        _db.ResumeTemplates.Add(template);
        await _db.SaveChangesAsync(ct);
        return template;
    }

    public async Task<ResumeTemplate> UpdateAsync(ResumeTemplate template, CancellationToken ct = default)
    {
        _db.ResumeTemplates.Update(template);
        await _db.SaveChangesAsync(ct);
        return template;
    }

    // Atomically increment UsageCount without loading the full entity
    public async Task UpdateUsageCountAsync(int templateId, CancellationToken ct = default)
        => await _db.ResumeTemplates
                    .Where(t => t.TemplateId == templateId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(t => t.UsageCount, t => t.UsageCount + 1), ct);
}
