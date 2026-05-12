using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAI.Resume.DTOs.Request;
using ResumeAI.Resume.DTOs.Response;
using ResumeAI.Resume.Extensions;
using ResumeAI.Resume.Services.Interfaces;

namespace ResumeAI.Resume.Controllers;

/// <summary>
/// Resume CRUD, duplication, publish/unpublish, ATS score update,
/// public gallery, and template-based queries.
/// Route: /api/resumes
/// </summary>
[ApiController]
[Route("api/resumes")]
// [Authorize]
[Produces("application/json")]
public class ResumeController : ControllerBase
{
    private readonly IResumeService _resumeService;
    private readonly ILogger<ResumeController> _logger;

    public ResumeController(IResumeService resumeService, ILogger<ResumeController> logger)
    {
        _resumeService = resumeService;
        _logger        = logger;
    }

    //   POST /api/resumes 
    /// <summary>
    /// Create a new resume from a template.
    /// FREE users are limited to 3 resumes. PREMIUM users have no limit.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ResumeResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateResumeRequest request, CancellationToken ct)
    {
        var userId           = User.GetUserId();
        var subscriptionPlan = User.GetSubscriptionPlan();

        var resume = await _resumeService.CreateResumeAsync(userId, subscriptionPlan, request, ct);
        return CreatedAtAction(nameof(GetById), new { resumeId = resume.ResumeId },
            ApiResponse<ResumeResponse>.Ok(resume, "Resume created successfully."));
    }

    //   GET /api/resumes/{resumeId}
    /// <summary>
    /// Get a resume by ID.
    /// Private resumes are only accessible by their owner.
    /// Public resumes are accessible by anyone and increment ViewCount.
    /// </summary>
    [HttpGet("{resumeId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ResumeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(int resumeId, CancellationToken ct)
    {
        // Anonymous users get userId = 0 — they can only see public resumes
        var requestingUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : 0;
        var resume = await _resumeService.GetResumeByIdAsync(resumeId, requestingUserId, ct);
        return Ok(ApiResponse<ResumeResponse>.Ok(resume));
    }

    //   GET /api/resumes/user/{userId} 
    /// <summary>
    /// Get all resumes belonging to a specific user.
    /// Users can only retrieve their own resumes. Admins can retrieve any user's resumes.
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IList<ResumeResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByUser(int userId, CancellationToken ct)
    {
        // Only allow own resumes unless admin
        if (User.GetUserId() != userId && !User.IsAdmin())
            return Forbid();

        var resumes = await _resumeService.GetResumesByUserAsync(userId, ct);
        return Ok(ApiResponse<IList<ResumeResponse>>.Ok(resumes));
    }

    //   GET /api/resumes/my 
    /// <summary>Get all resumes of the currently authenticated user.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<IList<ResumeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyResumes(CancellationToken ct)
    {
        var resumes = await _resumeService.GetResumesByUserAsync(User.GetUserId(), ct);
        return Ok(ApiResponse<IList<ResumeResponse>>.Ok(resumes));
    }

    //   PUT /api/resumes/{resumeId} 
    /// <summary>Update resume title, target job title, template, language, or status.</summary>
    [HttpPut("{resumeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ResumeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(int resumeId, [FromBody] UpdateResumeRequest request, CancellationToken ct)
    {
        var resume = await _resumeService.UpdateResumeAsync(resumeId, User.GetUserId(), request, ct);
        return Ok(ApiResponse<ResumeResponse>.Ok(resume, "Resume updated successfully."));
    }

    //   DELETE /api/resumes/{resumeId} 
    /// <summary>Permanently delete a resume. Only the owner can delete.</summary>
    [HttpDelete("{resumeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int resumeId, CancellationToken ct)
    {
        await _resumeService.DeleteResumeAsync(resumeId, User.GetUserId(), ct);
        return NoContent();
    }

    //   POST /api/resumes/{resumeId}/duplicate  
    /// <summary>
    /// Deep-copy a resume. The copy starts as DRAFT, IsPublic = false, AtsScore = 0.
    /// FREE users cannot duplicate if they already have 3 resumes.
    /// </summary>
    [HttpPost("{resumeId:int}/duplicate")]
    [ProducesResponseType(typeof(ApiResponse<ResumeResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Duplicate(int resumeId, CancellationToken ct)
    {
        var userId           = User.GetUserId();
        var subscriptionPlan = User.GetSubscriptionPlan();

        var copy = await _resumeService.DuplicateResumeAsync(resumeId, userId, subscriptionPlan, ct);
        return CreatedAtAction(nameof(GetById), new { resumeId = copy.ResumeId },
            ApiResponse<ResumeResponse>.Ok(copy, "Resume duplicated successfully."));
    }

    //   PUT /api/resumes/{resumeId}/ats-score 
    /// <summary>
    /// Update ATS compatibility score (0-100).
    /// Called internally by the AI Service after running an ATS check.
    /// Uses atomic ExecuteUpdateAsync — no full entity load.
    /// </summary>
    [HttpPut("{resumeId:int}/ats-score")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAtsScore(
        int resumeId, [FromBody] UpdateAtsScoreRequest request, CancellationToken ct)
    {
        await _resumeService.UpdateAtsScoreAsync(resumeId, User.GetUserId(), request.AtsScore, ct);
        return Ok(ApiResponse<object>.Ok(new { }, $"ATS score updated to {request.AtsScore}."));
    }

    //   PUT /api/resumes/{resumeId}/publish 
    /// <summary>Share a resume to the public gallery (IsPublic = true).</summary>
    [HttpPut("{resumeId:int}/publish")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Publish(int resumeId, CancellationToken ct)
    {
        await _resumeService.PublishResumeAsync(resumeId, User.GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Resume published to public gallery."));
    }

    //   PUT /api/resumes/{resumeId}/unpublish 
    /// <summary>Remove a resume from the public gallery (IsPublic = false).</summary>
    [HttpPut("{resumeId:int}/unpublish")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Unpublish(int resumeId, CancellationToken ct)
    {
        await _resumeService.UnpublishResumeAsync(resumeId, User.GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Resume removed from public gallery."));
    }

    //   GET /api/resumes/public 
    /// <summary>
    /// Browse all publicly shared resumes (the public gallery).
    /// Accessible without authentication.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IList<ResumeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic(CancellationToken ct)
    {
        var resumes = await _resumeService.GetPublicResumesAsync(ct);
        return Ok(ApiResponse<IList<ResumeResponse>>.Ok(resumes));
    }

    //   GET /api/resumes/template/{templateId}  
    /// <summary>Get all resumes using a specific template (Admin use).</summary>
    [HttpGet("template/{templateId:int}")]
    // [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<IList<ResumeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTemplate(int templateId, CancellationToken ct)
    {
        var resumes = await _resumeService.GetResumesByTemplateAsync(templateId, ct);
        return Ok(ApiResponse<IList<ResumeResponse>>.Ok(resumes));
    }
}
