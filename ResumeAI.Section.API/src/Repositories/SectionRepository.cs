using Microsoft.EntityFrameworkCore;
using ResumeAI.Section.Data;
using ResumeAI.Section.Entities;
using ResumeAI.Section.Enums;
using ResumeAI.Section.Repositories.Interfaces;

namespace ResumeAI.Section.Repositories;

public class SectionRepository : ISectionRepository
{
    private readonly SectionDbContext _db;

    public SectionRepository(SectionDbContext db) => _db = db;

    //   Queries 
    public async Task<IList<ResumeSection>> FindByResumeIdAsync(int resumeId, CancellationToken ct = default)
        => await _db.ResumeSections
                    .AsNoTracking()
                    .Where(s => s.ResumeId == resumeId)
                    .OrderBy(s => s.DisplayOrder)
                    .ToListAsync(ct);

    public async Task<ResumeSection?> FindByResumeIdAndSectionTypeAsync(
    int resumeId, string sectionType, CancellationToken ct = default)
{
    // Parse string to enum first — then compare enum to enum in SQL
    if (!Enum.TryParse<SectionType>(sectionType, ignoreCase: true, out var parsedType))
        return null;

    return await _db.ResumeSections
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ResumeId == resumeId &&
                                             s.SectionType == parsedType, ct);
}

    public async Task<ResumeSection?> FindBySectionIdAsync(int sectionId, CancellationToken ct = default)
        => await _db.ResumeSections
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId, ct);

    public async Task<IList<ResumeSection>> FindByResumeIdOrderByDisplayOrderAsync(
        int resumeId, CancellationToken ct = default)
        => await _db.ResumeSections
                    .AsNoTracking()
                    .Where(s => s.ResumeId == resumeId)
                    .OrderBy(s => s.DisplayOrder)
                    .ToListAsync(ct);

    public async Task<IList<ResumeSection>> FindByAiGeneratedAsync(bool aiGenerated, CancellationToken ct = default)
        => await _db.ResumeSections
                    .AsNoTracking()
                    .Where(s => s.AiGenerated == aiGenerated)
                    .ToListAsync(ct);

    public async Task<int> CountByResumeIdAsync(int resumeId, CancellationToken ct = default)
        => await _db.ResumeSections
                    .CountAsync(s => s.ResumeId == resumeId, ct);

    //   Commands 
    public async Task<ResumeSection> CreateAsync(ResumeSection section, CancellationToken ct = default)
    {
        section.CreatedAt = DateTime.UtcNow;
        section.UpdatedAt = DateTime.UtcNow;
        _db.ResumeSections.Add(section);
        await _db.SaveChangesAsync(ct);
        return section;
    }

    public async Task<ResumeSection> UpdateAsync(ResumeSection section, CancellationToken ct = default)
    {
        section.UpdatedAt = DateTime.UtcNow;
        _db.ResumeSections.Update(section);
        await _db.SaveChangesAsync(ct);
        return section;
    }

    public async Task DeleteBySectionIdAsync(int sectionId, CancellationToken ct = default)
        => await _db.ResumeSections
                    .Where(s => s.SectionId == sectionId)
                    .ExecuteDeleteAsync(ct);

    public async Task DeleteByResumeIdAsync(int resumeId, CancellationToken ct = default)
        => await _db.ResumeSections
                    .Where(s => s.ResumeId == resumeId)
                    .ExecuteDeleteAsync(ct);

    //   Atomic Updates 

    /// <summary>
    /// Updates only DisplayOrder column atomically using ExecuteUpdateAsync.
    /// Called in a loop by ReorderSections — no full entity load needed.
    /// </summary>
    public async Task UpdateDisplayOrderAsync(int sectionId, int displayOrder, CancellationToken ct = default)
        => await _db.ResumeSections
                    .Where(s => s.SectionId == sectionId)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(s => s.DisplayOrder, displayOrder)
                        .SetProperty(s => s.UpdatedAt,    DateTime.UtcNow), ct);

    /// <summary>
    /// Atomically toggles IsVisible using ExecuteUpdateAsync.
    /// </summary>
    public async Task ToggleVisibilityAsync(int sectionId, bool isVisible, CancellationToken ct = default)
        => await _db.ResumeSections
                    .Where(s => s.SectionId == sectionId)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(s => s.IsVisible,  isVisible)
                        .SetProperty(s => s.UpdatedAt,  DateTime.UtcNow), ct);

    //   Bulk Update 

    /// <summary>
    /// Batch update multiple sections in a single SaveChangesAsync call.
    /// Uses EF Core ChangeTracker — attaches each entity and marks as Modified.
    /// </summary>
    public async Task<IList<ResumeSection>> BulkUpdateAsync(
        IList<ResumeSection> sections, CancellationToken ct = default)
    {
        foreach (var section in sections)
        {
            section.UpdatedAt = DateTime.UtcNow;
            _db.ResumeSections.Update(section);
        }

        // Single SaveChangesAsync — EF Core batches all UPDATEs
        await _db.SaveChangesAsync(ct);
        return sections;
    }
}
