using ResumeAI.Section.DTOs.Request;
using ResumeAI.Section.DTOs.Response;

namespace ResumeAI.Section.Services.Interfaces;

public interface ISectionService
{
    /// <summary>Add a new section to a resume. Owner verified via userId.</summary>
    Task<SectionResponse> AddSectionAsync(int userId, AddSectionRequest request, CancellationToken ct = default);

    /// <summary>Get all sections for a resume ordered by DisplayOrder.</summary>
    Task<IList<SectionResponse>> GetSectionsByResumeAsync(int resumeId, int userId, CancellationToken ct = default);

    /// <summary>Get a single section by ID.</summary>
    Task<SectionResponse> GetSectionByIdAsync(int sectionId, int userId, CancellationToken ct = default);

    /// <summary>Update a section's content, title, type, visibility, or AiGenerated flag.</summary>
    Task<SectionResponse> UpdateSectionAsync(int sectionId, int userId, UpdateSectionRequest request, CancellationToken ct = default);

    /// <summary>Permanently delete a single section.</summary>
    Task DeleteSectionAsync(int sectionId, int userId, CancellationToken ct = default);

    /// <summary>
    /// Reorder sections via drag-and-drop.
    /// Accepts ordered list of section IDs — position = new DisplayOrder.
    /// Uses ExecuteUpdateAsync in a loop for atomic per-row updates.
    /// </summary>
    Task ReorderSectionsAsync(int resumeId, int userId, ReorderSectionsRequest request, CancellationToken ct = default);

    /// <summary>Toggle IsVisible for a section (show/hide without deleting).</summary>
    Task ToggleVisibilityAsync(int sectionId, int userId, CancellationToken ct = default);

    /// <summary>Delete ALL sections belonging to a resume.</summary>
    Task DeleteAllSectionsAsync(int resumeId, int userId, CancellationToken ct = default);

    /// <summary>Get sections filtered by SectionType for a specific resume.</summary>
    Task<SectionResponse?> GetSectionByTypeAsync(int resumeId, string sectionType, int userId, CancellationToken ct = default);

    /// <summary>
    /// Batch update multiple sections in a single DB round-trip.
    /// Used by live resume editor to save all changes at once.
    /// Uses EF Core ChangeTracker — single SaveChangesAsync call.
    /// </summary>
    Task<IList<SectionResponse>> BulkUpdateSectionsAsync(int resumeId, int userId, BulkUpdateSectionsRequest request, CancellationToken ct = default);
}
