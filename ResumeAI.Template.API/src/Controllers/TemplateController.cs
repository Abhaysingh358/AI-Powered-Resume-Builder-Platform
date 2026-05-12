using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAI.Template.DTOs.Request;
using ResumeAI.Template.DTOs.Response;
using ResumeAI.Template.Services.Interfaces;

namespace ResumeAI.Template.Controllers;

/// <summary>
/// Template library management.
/// Admin: create, update, deactivate.
/// Users: browse, filter by tier/category, get by ID, increment usage.
/// Route: /api/templates
/// </summary>
[ApiController]
[Route("api/templates")]
[Produces("application/json")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplateController> _logger;

    public TemplateController(ITemplateService templateService, ILogger<TemplateController> logger)
    {
        _templateService = templateService;
        _logger          = logger;
    }

    // POST /api/templates
    /// <summary>Create a new template. Admin only.</summary>
    [HttpPost]
    // [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequest request, CancellationToken ct)
    {
        var template = await _templateService.CreateTemplateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { templateId = template.TemplateId },
            ApiResponse<TemplateResponse>.Ok(template, "Template created."));
    }

    // GET /api/templates
    /// <summary>Get all active templates. Returns lightweight list without HtmlLayout and CssStyles.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IList<TemplateListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var templates = await _templateService.GetAllTemplatesAsync(ct);
        return Ok(ApiResponse<IList<TemplateListResponse>>.Ok(templates));
    }

    // GET /api/templates/{templateId}
    /// <summary>Get full template details including HtmlLayout and CssStyles. Used when user selects a template.</summary>
    [HttpGet("{templateId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int templateId, CancellationToken ct)
    {
        var template = await _templateService.GetTemplateByIdAsync(templateId, ct);
        if (template is null)
            return NotFound(ApiResponse<object>.Fail($"Template {templateId} not found."));

        return Ok(ApiResponse<TemplateResponse>.Ok(template));
    }

    // GET /api/templates/free
    /// <summary>Get all free-tier templates. Available to all users including guests.</summary>
    [HttpGet("free")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IList<TemplateListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFree(CancellationToken ct)
    {
        var templates = await _templateService.GetFreeTemplatesAsync(ct);
        return Ok(ApiResponse<IList<TemplateListResponse>>.Ok(templates));
    }

    // GET /api/templates/premium
    /// <summary>Get all premium-tier templates. List is visible to all; usage requires Premium subscription.</summary>
    [HttpGet("premium")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IList<TemplateListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPremium(CancellationToken ct)
    {
        var templates = await _templateService.GetPremiumTemplatesAsync(ct);
        return Ok(ApiResponse<IList<TemplateListResponse>>.Ok(templates));
    }

    // GET /api/templates/popular
    /// <summary>Get the top 10 most-used templates ordered by UsageCount descending.</summary>
    [HttpGet("popular")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IList<TemplateListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopular(CancellationToken ct)
    {
        var templates = await _templateService.GetPopularTemplatesAsync(ct);
        return Ok(ApiResponse<IList<TemplateListResponse>>.Ok(templates));
    }

    // GET /api/templates/category/{category}
    /// <summary>
    /// Filter templates by category.
    /// Valid values: PROFESSIONAL, CREATIVE, MODERN, MINIMALIST, ATS_OPTIMISED
    /// </summary>
    [HttpGet("category/{category}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IList<TemplateListResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCategory(string category, CancellationToken ct)
    {
        var templates = await _templateService.GetByCategoryAsync(category, ct);
        return Ok(ApiResponse<IList<TemplateListResponse>>.Ok(templates));
    }

    // PUT /api/templates/{templateId}
    /// <summary>Update a template's name, description, layout, styles, category, or tier. Admin only.</summary>
    [HttpPut("{templateId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int templateId, [FromBody] UpdateTemplateRequest request, CancellationToken ct)
    {
        var template = await _templateService.UpdateTemplateAsync(templateId, request, ct);
        return Ok(ApiResponse<TemplateResponse>.Ok(template, "Template updated."));
    }

    // PUT /api/templates/{templateId}/deactivate
    /// <summary>Soft-delete a template (IsActive = false). Admin only.</summary>
    [HttpPut("{templateId:int}/deactivate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int templateId, CancellationToken ct)
    {
        await _templateService.DeactivateTemplateAsync(templateId, ct);
        return Ok(ApiResponse<object>.Ok(new { }, $"Template {templateId} deactivated."));
    }

    // PUT /api/templates/{templateId}/increment-usage
    /// <summary>
    /// Increment UsageCount by 1 when a user selects this template to create a resume.
    /// Uses ExecuteUpdateAsync for atomic update without loading the full entity.
    /// Called by Resume Service or frontend when a resume is created from this template.
    /// </summary>
    [HttpPut("{templateId:int}/increment-usage")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IncrementUsage(int templateId, CancellationToken ct)
    {
        await _templateService.IncrementUsageAsync(templateId, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Usage count updated."));
    }
}
