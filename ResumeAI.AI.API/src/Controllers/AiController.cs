using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAI.AI.DTOs.Request;
using ResumeAI.AI.DTOs.Response;
using ResumeAI.AI.Extensions;
using ResumeAI.AI.Services.Interfaces;

namespace ResumeAI.AI.Controllers;

/// <summary>
/// AI content generation: summary, bullets, cover letter, section improvement,
/// ATS check, skill suggestions, resume tailoring, translation, history, and quota.
/// Route: /api/ai
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
[Produces("application/json")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly IMapper    _mapper;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiService aiService, IMapper mapper, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _mapper    = mapper;
        _logger    = logger;
    }

    // POST /api/ai/generate-summary
    /// <summary>
    /// Generate a professional summary based on job title, years of experience, and key skills.
    /// FREE users: counts against 5 AI calls/month limit.
    /// </summary>
    [HttpPost("generate-summary")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerateSummary([FromBody] GenerateSummaryRequest request, CancellationToken ct)
    {
        var result = await _aiService.GenerateSummaryAsync(User.GetUserId(), User.GetSubscriptionPlan(), request, ct);
        return Ok(ApiResponse<string>.Ok(result, "Summary generated successfully."));
    }

    // POST /api/ai/generate-bullets
    /// <summary>
    /// Generate impactful bullet points for a work experience entry.
    /// FREE users: counts against 5 AI calls/month limit.
    /// </summary>
    [HttpPost("generate-bullets")]
    [ProducesResponseType(typeof(ApiResponse<IList<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerateBullets([FromBody] GenerateBulletsRequest request, CancellationToken ct)
    {
        var result = await _aiService.GenerateBulletPointsAsync(User.GetUserId(), User.GetSubscriptionPlan(), request, ct);
        return Ok(ApiResponse<IList<string>>.Ok(result, "Bullet points generated."));
    }

    // POST /api/ai/generate-cover-letter
    /// <summary>
    /// Generate a personalised cover letter for a specific job description.
    /// PREMIUM users only.
    /// </summary>
    [HttpPost("generate-cover-letter")]
    [Authorize(Policy = "PremiumOnly")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateCoverLetter([FromBody] GenerateCoverLetterRequest request, CancellationToken ct)
    {
        var result = await _aiService.GenerateCoverLetterAsync(User.GetUserId(), User.GetSubscriptionPlan(), request, ct);
        return Ok(ApiResponse<string>.Ok(result, "Cover letter generated."));
    }

    // POST /api/ai/improve-section
    /// <summary>
    /// Improve or rewrite any resume section for a more impactful tone.
    /// PREMIUM users only.
    /// </summary>
    [HttpPost("improve-section")]
    [Authorize(Policy = "PremiumOnly")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImproveSection([FromBody] ImproveSectionRequest request, CancellationToken ct)
    {
        var result = await _aiService.ImproveSectionAsync(User.GetUserId(), User.GetSubscriptionPlan(), request, ct);
        return Ok(ApiResponse<string>.Ok(result, "Section improved."));
    }

    // POST /api/ai/check-ats
    /// <summary>
    /// Run an ATS compatibility check scoring resume against a job description (0-100).
    /// FREE users: 3 ATS checks/month. After completion, updates ATS score in Resume Service.
    /// </summary>
    [HttpPost("check-ats")]
    [ProducesResponseType(typeof(ApiResponse<AtsReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckAts([FromBody] CheckAtsRequest request, CancellationToken ct)
    {
        var result = await _aiService.CheckAtsCompatibilityAsync(User.GetUserId(), User.GetSubscriptionPlan(), request, ct);
        return Ok(ApiResponse<AtsReportResponse>.Ok(result, "ATS check completed."));
    }

    // POST /api/ai/suggest-skills
    /// <summary>
    /// Get AI-suggested skills based on the target job title.
    /// FREE users: counts against 5 AI calls/month limit.
    /// </summary>
    [HttpPost("suggest-skills")]
    [ProducesResponseType(typeof(ApiResponse<IList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SuggestSkills([FromBody] SuggestSkillsRequest request, CancellationToken ct)
    {
        var result = await _aiService.SuggestSkillsAsync(User.GetUserId(), User.GetSubscriptionPlan(), request, ct);
        return Ok(ApiResponse<IList<string>>.Ok(result, "Skills suggested."));
    }

    // POST /api/ai/tailor-for-job
    /// <summary>
    /// Tailor the entire resume to a specific job posting.
    /// Resume is serialised to JSON, sent to AI, revised resume returned.
    /// PREMIUM users only.
    /// </summary>
    [HttpPost("tailor-for-job")]
    [Authorize(Policy = "PremiumOnly")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TailorForJob([FromBody] TailorResumeRequest request, CancellationToken ct)
    {
        var result = await _aiService.TailorResumeForJobAsync(User.GetUserId(), User.GetSubscriptionPlan(), request, ct);
        return Ok(ApiResponse<string>.Ok(result, "Resume tailored for job."));
    }

    // POST /api/ai/translate
    /// <summary>
    /// Translate the full resume into another language.
    /// PREMIUM users only.
    /// </summary>
    [HttpPost("translate")]
    [Authorize(Policy = "PremiumOnly")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Translate([FromBody] TranslateResumeRequest request, CancellationToken ct)
    {
        var result = await _aiService.TranslateResumeAsync(User.GetUserId(), User.GetSubscriptionPlan(), request, ct);
        return Ok(ApiResponse<string>.Ok(result, "Resume translated."));
    }

    // GET /api/ai/history
    /// <summary>
    /// Get the full history of AI requests made by the authenticated user.
    /// Shows request type, model used, tokens consumed, and status.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<IList<AiRequestResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var history = await _aiService.GetAiHistoryAsync(User.GetUserId(), ct);
        var mapped  = _mapper.Map<IList<AiRequestResponse>>(history);
        return Ok(ApiResponse<IList<AiRequestResponse>>.Ok(mapped));
    }

    // GET /api/ai/quota
    /// <summary>
    /// Check remaining monthly AI call quota.
    /// FREE users see their remaining calls and ATS checks.
    /// PREMIUM users see unlimited (-1).
    /// </summary>
    [HttpGet("quota")]
    [ProducesResponseType(typeof(ApiResponse<QuotaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuota(CancellationToken ct)
    {
        var quota = await _aiService.GetRemainingQuotaAsync(User.GetUserId(), User.GetSubscriptionPlan(), ct);
        return Ok(ApiResponse<QuotaResponse>.Ok(quota));
    }
}
