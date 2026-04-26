using AutoMapper;
using ResumeAI.Section.DTOs.Request;
using ResumeAI.Section.DTOs.Response;
using ResumeAI.Section.Entities;
using ResumeAI.Section.Enums;
using ResumeAI.Section.Repositories.Interfaces;
using ResumeAI.Section.Services.Interfaces;

namespace ResumeAI.Section.Services;

public class SectionService : ISectionService
{
    private readonly ISectionRepository      _sectionRepo;
    private readonly IMapper                 _mapper;
    private readonly ILogger<SectionService> _logger;

    public SectionService(
        ISectionRepository sectionRepo,
        IMapper mapper,
        ILogger<SectionService> logger)
    {
        _sectionRepo = sectionRepo;
        _mapper      = mapper;
        _logger      = logger;
    }

    // ── Add Section 
    public async Task<SectionResponse> AddSectionAsync(
        int userId, AddSectionRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<SectionType>(request.SectionType, ignoreCase: true, out var sectionType))
            throw new ArgumentException($"Invalid SectionType '{request.SectionType}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames<SectionType>())}");

        var section = new ResumeSection
        {
            ResumeId     = request.ResumeId,
            UserId       = userId,
            SectionType  = sectionType,
            Title        = request.Title.Trim(),
            Content      = request.Content,
            DisplayOrder = request.DisplayOrder,
            IsVisible    = request.IsVisible,
            AiGenerated  = false
        };

        var created = await _sectionRepo.CreateAsync(section, ct);
        _logger.LogInformation("Section {SectionId} added to Resume {ResumeId} by user {UserId}",
            created.SectionId, request.ResumeId, userId);

        return _mapper.Map<SectionResponse>(created);
    }

    // ── Get Sections By Resume 
    public async Task<IList<SectionResponse>> GetSectionsByResumeAsync(
        int resumeId, int userId, CancellationToken ct = default)
    {
        var sections = await _sectionRepo.FindByResumeIdOrderByDisplayOrderAsync(resumeId, ct);
        return _mapper.Map<IList<SectionResponse>>(sections);
    }

    // ── Get Section By ID 
    public async Task<SectionResponse> GetSectionByIdAsync(
        int sectionId, int userId, CancellationToken ct = default)
    {
        var section = await RequireOwnerAsync(sectionId, userId, ct);
        return _mapper.Map<SectionResponse>(section);
    }

    // ── Update Section 
    public async Task<SectionResponse> UpdateSectionAsync(
        int sectionId, int userId, UpdateSectionRequest request, CancellationToken ct = default)
    {
        var section = await RequireOwnerAsync(sectionId, userId, ct);

        if (!string.IsNullOrWhiteSpace(request.Title))
            section.Title = request.Title.Trim();

        if (request.Content is not null)
            section.Content = request.Content;

        if (!string.IsNullOrWhiteSpace(request.SectionType) &&
            Enum.TryParse<SectionType>(request.SectionType, ignoreCase: true, out var sectionType))
            section.SectionType = sectionType;

        if (request.DisplayOrder.HasValue)
            section.DisplayOrder = request.DisplayOrder.Value;

        if (request.IsVisible.HasValue)
            section.IsVisible = request.IsVisible.Value;

        // AiGenerated flag — set by AI Service when generating content
        // Reset to false when user manually edits
        if (request.AiGenerated.HasValue)
            section.AiGenerated = request.AiGenerated.Value;

        var updated = await _sectionRepo.UpdateAsync(section, ct);
        _logger.LogInformation("Section {SectionId} updated by user {UserId}", sectionId, userId);

        return _mapper.Map<SectionResponse>(updated);
    }

    // ── Delete Section 
    public async Task DeleteSectionAsync(int sectionId, int userId, CancellationToken ct = default)
    {
        await RequireOwnerAsync(sectionId, userId, ct);
        await _sectionRepo.DeleteBySectionIdAsync(sectionId, ct);
        _logger.LogInformation("Section {SectionId} deleted by user {UserId}", sectionId, userId);
    }

    // ── Reorder Sections 
    /// <summary>
    /// Accepts ordered list of section IDs.
    /// Loops and calls ExecuteUpdateAsync for each — atomic per-row update.
    /// Position in the list = new DisplayOrder value.
    /// </summary>
    public async Task ReorderSectionsAsync(
        int resumeId, int userId, ReorderSectionsRequest request, CancellationToken ct = default)
    {
        // Verify all sections belong to this user and resume
        var sections = await _sectionRepo.FindByResumeIdAsync(resumeId, ct);
        var sectionIds = sections.Select(s => s.SectionId).ToHashSet();

        foreach (var id in request.OrderedSectionIds)
        {
            if (!sectionIds.Contains(id))
                throw new ArgumentException($"Section {id} does not belong to Resume {resumeId}.");
        }

        // Loop and atomically update DisplayOrder for each section
        for (int i = 0; i < request.OrderedSectionIds.Count; i++)
        {
            await _sectionRepo.UpdateDisplayOrderAsync(request.OrderedSectionIds[i], i, ct);
        }

        _logger.LogInformation("Sections reordered for Resume {ResumeId} by user {UserId}",
            resumeId, userId);
    }

    // ── Toggle Visibility 
    public async Task ToggleVisibilityAsync(int sectionId, int userId, CancellationToken ct = default)
    {
        var section = await RequireOwnerAsync(sectionId, userId, ct);
        var newVisibility = !section.IsVisible;

        await _sectionRepo.ToggleVisibilityAsync(sectionId, newVisibility, ct);
        _logger.LogInformation("Section {SectionId} visibility toggled to {Visible} by user {UserId}",
            sectionId, newVisibility, userId);
    }

    // ── Delete All Sections 
    public async Task DeleteAllSectionsAsync(int resumeId, int userId, CancellationToken ct = default)
    {
        await _sectionRepo.DeleteByResumeIdAsync(resumeId, ct);
        _logger.LogInformation("All sections deleted for Resume {ResumeId} by user {UserId}",
            resumeId, userId);
    }

    // ── Get By Type 
  public async Task<SectionResponse?> GetSectionByTypeAsync(
    int resumeId, string sectionType, int userId, CancellationToken ct = default)
{
    if (!Enum.TryParse<SectionType>(sectionType, ignoreCase: true, out _))
        throw new ArgumentException(
            $"Invalid SectionType '{sectionType}'. " +
            $"Valid values: {string.Join(", ", Enum.GetNames<SectionType>())}");

    var section = await _sectionRepo.FindByResumeIdAndSectionTypeAsync(resumeId, sectionType, ct);
    return section is null ? null : _mapper.Map<SectionResponse>(section);
}

    // ── Bulk Update 
    /// <summary>
    /// Batch update multiple sections in one DB round-trip.
    /// Loads existing sections, applies changes, then calls BulkUpdateAsync
    /// which uses EF Core ChangeTracker with a single SaveChangesAsync.
    /// </summary>
    public async Task<IList<SectionResponse>> BulkUpdateSectionsAsync(
        int resumeId, int userId, BulkUpdateSectionsRequest request, CancellationToken ct = default)
    {
        // Load all sections for this resume
        var existing = await _sectionRepo.FindByResumeIdAsync(resumeId, ct);
        var existingMap = existing.ToDictionary(s => s.SectionId);

        var toUpdate = new List<ResumeSection>();

        foreach (var item in request.Sections)
        {
            if (!existingMap.TryGetValue(item.SectionId, out var section))
                throw new KeyNotFoundException($"Section {item.SectionId} not found in Resume {resumeId}.");

            if (section.UserId != userId)
                throw new UnauthorizedAccessException(
                    $"Section {item.SectionId} does not belong to you.");

            // Apply changes
            if (!string.IsNullOrWhiteSpace(item.Title))    section.Title        = item.Title.Trim();
            if (item.Content is not null)                   section.Content      = item.Content;
            if (item.DisplayOrder.HasValue)                 section.DisplayOrder = item.DisplayOrder.Value;
            if (item.IsVisible.HasValue)                    section.IsVisible    = item.IsVisible.Value;
            if (item.AiGenerated.HasValue)                  section.AiGenerated  = item.AiGenerated.Value;

            toUpdate.Add(section);
        }

        // Single SaveChangesAsync — EF Core batches all UPDATEs
        var updated = await _sectionRepo.BulkUpdateAsync(toUpdate, ct);
        _logger.LogInformation("Bulk updated {Count} sections for Resume {ResumeId} by user {UserId}",
            updated.Count, resumeId, userId);

        return _mapper.Map<IList<SectionResponse>>(updated);
    }

    // ==========================================================================
    // Helpers
    // ==========================================================================
    private async Task<ResumeSection> RequireOwnerAsync(int sectionId, int userId, CancellationToken ct)
    {
        var section = await _sectionRepo.FindBySectionIdAsync(sectionId, ct)
            ?? throw new KeyNotFoundException($"Section {sectionId} not found.");

        if (section.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to access this section.");

        return section;
    }
}
