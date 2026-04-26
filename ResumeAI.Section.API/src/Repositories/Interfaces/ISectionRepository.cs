using ResumeAI.Section.Entities;

namespace ResumeAI.Section.Repositories.Interfaces;

public interface ISectionRepository
{
    //   Queries 
    Task<IList<ResumeSection>> FindByResumeIdAsync(int resumeId, CancellationToken ct = default);

    Task<ResumeSection?> FindByResumeIdAndSectionTypeAsync(int resumeId, string sectionType, CancellationToken ct = default);

    Task<ResumeSection?> FindBySectionIdAsync(int sectionId, CancellationToken ct = default);

    /// <summary>Returns sections sorted by DisplayOrder ascending — used for live preview.</summary>
    Task<IList<ResumeSection>> FindByResumeIdOrderByDisplayOrderAsync(int resumeId, CancellationToken ct = default);

    Task<IList<ResumeSection>> FindByAiGeneratedAsync(bool aiGenerated, CancellationToken ct = default);

    Task<int> CountByResumeIdAsync(int resumeId, CancellationToken ct = default);

    //   Commands 
    Task<ResumeSection> CreateAsync(ResumeSection section, CancellationToken ct = default);

    Task<ResumeSection> UpdateAsync(ResumeSection section, CancellationToken ct = default);

    Task DeleteBySectionIdAsync(int sectionId, CancellationToken ct = default);

    Task DeleteByResumeIdAsync(int resumeId, CancellationToken ct = default);

    //   Atomic Updates 
    /// <summary>
    /// Atomically update DisplayOrder for a single section.
    /// Used in the ReorderSections loop via ExecuteUpdateAsync.
    /// </summary>
    Task UpdateDisplayOrderAsync(int sectionId, int displayOrder, CancellationToken ct = default);

    /// <summary>
    /// Atomically toggle IsVisible for a single section.
    /// </summary>
    Task ToggleVisibilityAsync(int sectionId, bool isVisible, CancellationToken ct = default);

    //   Bulk 
    /// <summary>
    /// Batch update multiple sections in one SaveChangesAsync call.
    /// Uses EF Core ChangeTracker — caller attaches entities before calling.
    /// </summary>
    Task<IList<ResumeSection>> BulkUpdateAsync(IList<ResumeSection> sections, CancellationToken ct = default);
}
