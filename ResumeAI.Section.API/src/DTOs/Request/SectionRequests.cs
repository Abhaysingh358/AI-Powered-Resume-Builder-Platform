using System.ComponentModel.DataAnnotations;

namespace ResumeAI.Section.DTOs.Request;

//   Add Section 
public record AddSectionRequest
{
    [Required]
    public int ResumeId { get; init; }

    [Required]
    [MaxLength(30)]
    public string SectionType { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    /// <summary>Rich text JSON string — stored as-is.</summary>
    public string? Content { get; init; }

    public int DisplayOrder { get; init; } = 0;

    public bool IsVisible { get; init; } = true;
}

//   Update Section 
public record UpdateSectionRequest
{
    [MaxLength(200)]
    public string? Title { get; init; }

    /// <summary>Rich text JSON string — stored as-is.</summary>
    public string? Content { get; init; }

    [MaxLength(30)]
    public string? SectionType { get; init; }

    public int? DisplayOrder { get; init; }

    public bool? IsVisible { get; init; }

    /// <summary>
    /// Set to true by AI Service when it generates content for this section.
    /// Set to false when user manually edits the content.
    /// </summary>
    public bool? AiGenerated { get; init; }
}

//   Reorder Sections 
/// <summary>
/// Accepts an ordered list of section IDs.
/// The position in the list determines the new DisplayOrder value.
/// Example: [3, 1, 5] means SectionId=3 gets order=0, SectionId=1 gets order=1, etc.
/// </summary>
public record ReorderSectionsRequest
{
    [Required]
    public IList<int> OrderedSectionIds { get; init; } = [];
}

//   Bulk Update Sections 
/// <summary>
/// Batch update multiple sections in a single SaveChangesAsync call.
/// Used by the live resume editor to save all section changes at once.
/// </summary>
public record BulkUpdateSectionItem
{
    [Required]
    public int SectionId { get; init; }

    [MaxLength(200)]
    public string? Title { get; init; }

    public string? Content { get; init; }

    public int? DisplayOrder { get; init; }

    public bool? IsVisible { get; init; }

    public bool? AiGenerated { get; init; }
}

public record BulkUpdateSectionsRequest
{
    [Required]
    public IList<BulkUpdateSectionItem> Sections { get; init; } = [];
}
