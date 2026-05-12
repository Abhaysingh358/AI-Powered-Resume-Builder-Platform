using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAI.Section.DTOs.Request;
using ResumeAI.Section.DTOs.Response;
using ResumeAI.Section.Extensions;
using ResumeAI.Section.Services.Interfaces;

namespace ResumeAI.Section.Controllers;

/// <summary>
/// Resume section CRUD, drag-and-drop reordering, visibility toggle,
/// bulk update, and type-based retrieval.
/// Route: /api/sections
/// </summary>
[ApiController]
[Route("api/sections")]
[Authorize]
[Produces("application/json")]
public class SectionController : ControllerBase
{
    private readonly ISectionService _sectionService;
    private readonly ILogger<SectionController> _logger;

    public SectionController(ISectionService sectionService, ILogger<SectionController> logger)
    {
        _sectionService = sectionService;
        _logger         = logger;
    }

    //   POST /api/sections 
    /// <summary>
    /// Add a new section to a resume.
    /// SectionType must be one of: SUMMARY, EXPERIENCE, EDUCATION, SKILLS,
    /// CERTIFICATIONS, PROJECTS, LANGUAGES, VOLUNTEER, CUSTOM.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SectionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AddSectionRequest request, CancellationToken ct)
    {
        var section = await _sectionService.AddSectionAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { sectionId = section.SectionId },
            ApiResponse<SectionResponse>.Ok(section, "Section added successfully."));
    }

    //   GET /api/sections/resume/{resumeId} 
    /// <summary>
    /// Get all sections for a resume ordered by DisplayOrder ascending.
    /// Used to render the live resume preview.
    /// </summary>
    [HttpGet("resume/{resumeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IList<SectionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByResume(int resumeId, CancellationToken ct)
    {
        var sections = await _sectionService.GetSectionsByResumeAsync(resumeId, User.GetUserId(), ct);
        return Ok(ApiResponse<IList<SectionResponse>>.Ok(sections));
    }

    //   GET /api/sections/{sectionId} 
    /// <summary>Get a single section by its ID.</summary>
    [HttpGet("{sectionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<SectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int sectionId, CancellationToken ct)
    {
        var section = await _sectionService.GetSectionByIdAsync(sectionId, User.GetUserId(), ct);
        return Ok(ApiResponse<SectionResponse>.Ok(section));
    }

    //   PUT /api/sections/{sectionId} 
    /// <summary>
    /// Update a section's title, content, type, visibility, or AiGenerated flag.
    /// When AI Service generates content, it sets AiGenerated = true.
    /// When user manually edits, frontend should set AiGenerated = false.
    /// </summary>
    [HttpPut("{sectionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<SectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        int sectionId, [FromBody] UpdateSectionRequest request, CancellationToken ct)
    {
        var section = await _sectionService.UpdateSectionAsync(sectionId, User.GetUserId(), request, ct);
        return Ok(ApiResponse<SectionResponse>.Ok(section, "Section updated successfully."));
    }

    //   DELETE /api/sections/{sectionId} 
    /// <summary>Permanently delete a single section.</summary>
    [HttpDelete("{sectionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int sectionId, CancellationToken ct)
    {
        await _sectionService.DeleteSectionAsync(sectionId, User.GetUserId(), ct);
        return NoContent();
    }

    //   PUT /api/sections/resume/{resumeId}/reorder
    /// <summary>
    /// Reorder sections via drag-and-drop.
    /// Send the full ordered list of section IDs — position = new DisplayOrder.
    /// Example body: { "orderedSectionIds": [3, 1, 5, 2] }
    /// Section 3 gets DisplayOrder=0, section 1 gets DisplayOrder=1, etc.
    /// Uses ExecuteUpdateAsync in a loop — atomic per-row update.
    /// </summary>
    [HttpPut("resume/{resumeId:int}/reorder")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reorder(
        int resumeId, [FromBody] ReorderSectionsRequest request, CancellationToken ct)
    {
        await _sectionService.ReorderSectionsAsync(resumeId, User.GetUserId(), request, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Sections reordered successfully."));
    }

    //   PUT /api/sections/{sectionId}/toggle-visibility 
    /// <summary>
    /// Toggle IsVisible for a section.
    /// Hides or shows the section in the resume preview without deleting it.
    /// </summary>
    [HttpPut("{sectionId:int}/toggle-visibility")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleVisibility(int sectionId, CancellationToken ct)
    {
        await _sectionService.ToggleVisibilityAsync(sectionId, User.GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Section visibility toggled."));
    }

    //   DELETE /api/sections/resume/{resumeId}/all 
    /// <summary>
    /// Delete ALL sections belonging to a resume.
    /// Used when deleting a resume or resetting it entirely.
    /// </summary>
    [HttpDelete("resume/{resumeId:int}/all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAll(int resumeId, CancellationToken ct)
    {
        await _sectionService.DeleteAllSectionsAsync(resumeId, User.GetUserId(), ct);
        return NoContent();
    }

    //   GET /api/sections/resume/{resumeId}/type/{sectionType} 
    /// <summary>
    /// Get a section by its type for a specific resume.
    /// Example: GET /api/sections/resume/5/type/SUMMARY
    /// Returns null if no section of that type exists.
    /// </summary>
    [HttpGet("resume/{resumeId:int}/type/{sectionType}")]
    [ProducesResponseType(typeof(ApiResponse<SectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByType(int resumeId, string sectionType, CancellationToken ct)
    {
        var section = await _sectionService.GetSectionByTypeAsync(resumeId, sectionType, User.GetUserId(), ct);
        if (section is null)
            return NotFound(ApiResponse<object>.Fail($"No {sectionType} section found for Resume {resumeId}."));

        return Ok(ApiResponse<SectionResponse>.Ok(section));
    }

    //   PUT /api/sections/resume/{resumeId}/bulk 
    /// <summary>
    /// Batch update multiple sections in one DB round-trip.
    /// Used by the live resume editor to save all section changes at once.
    /// Uses EF Core ChangeTracker — single SaveChangesAsync call for all updates.
    /// </summary>
    [HttpPut("resume/{resumeId:int}/bulk")]
    [ProducesResponseType(typeof(ApiResponse<IList<SectionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkUpdate(
        int resumeId, [FromBody] BulkUpdateSectionsRequest request, CancellationToken ct)
    {
        var sections = await _sectionService.BulkUpdateSectionsAsync(resumeId, User.GetUserId(), request, ct);
        return Ok(ApiResponse<IList<SectionResponse>>.Ok(sections, $"{sections.Count} sections updated."));
    }
}
