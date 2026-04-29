using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAI.Export.DTOs.Request;
using ResumeAI.Export.DTOs.Response;
using ResumeAI.Export.Extensions;
using ResumeAI.Export.Services.Interfaces;

namespace ResumeAI.Export.Controllers;

/// <summary>
/// Resume export to PDF, DOCX, and JSON.
/// Files are generated in memory and streamed directly to the browser.
/// Export job records are tracked in the database for history and stats.
/// Route: /api/exports
/// </summary>
[ApiController]
[Route("api/exports")]
[Authorize]
[Produces("application/json")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(
        IExportService exportService,
        ILogger<ExportController> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    // POST /api/exports/pdf

    /// <summary>
    /// Export resume to PDF using QuestPDF.
    /// File is generated in memory and streamed directly to the browser for download.
    /// FREE users: limited to 10 PDF exports per day.
    /// PREMIUM users: unlimited.
    /// </summary>
    [HttpPost("pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ExportPdf(
        [FromBody] ExportPdfRequest request,
        CancellationToken ct)
    {
        var (fileBytes, job) = await _exportService.ExportToPdfAsync(
            User.GetUserId(),
            User.GetSubscriptionPlan(),
            request,
            ct);

        var fileName = $"resume_{request.ResumeId}_{DateTime.UtcNow:yyyyMMdd}.pdf";

        return File(fileBytes, "application/pdf", fileName);
    }

    // POST /api/exports/docx

    /// <summary>
    /// Export resume to DOCX using DocumentFormat.OpenXml (OpenXML SDK).
    /// File is generated in memory and streamed directly to the browser.
    /// Available to all authenticated users.
    /// </summary>
    [HttpPost("docx")]
    // [Authorize(Policy = "PremiumOnly")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportDocx(
        [FromBody] ExportDocxRequest request,
        CancellationToken ct)
    {
        var (fileBytes, job) = await _exportService.ExportToDocxAsync(
            User.GetUserId(),
            request,
            ct);

        var fileName = $"resume_{request.ResumeId}_{DateTime.UtcNow:yyyyMMdd}.docx";

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            fileName);
    }

    // POST /api/exports/json

    /// <summary>
    /// Export resume to machine-readable JSON using System.Text.Json.
    /// Available to Premium users only.
    /// </summary>
    [HttpPost("json")]
    // [Authorize(Policy = "PremiumOnly")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportJson(
        [FromBody] ExportJsonRequest request,
        CancellationToken ct)
    {
        var (fileBytes, job) = await _exportService.ExportToJsonAsync(
            User.GetUserId(),
            request,
            ct);

        var fileName = $"resume_{request.ResumeId}_{DateTime.UtcNow:yyyyMMdd}.json";

        return File(fileBytes, "application/json", fileName);
    }

    // GET /api/exports/status/{jobId}

    /// <summary>
    /// Get the status of a specific export job by its ID.
    /// </summary>
    [HttpGet("status/{jobId}")]
    [ProducesResponseType(typeof(ApiResponse<ExportJobResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobStatus(
        string jobId,
        CancellationToken ct)
    {
        var job = await _exportService.GetJobStatusAsync(jobId, ct);

        if (job is null)
        {
            return NotFound(
                ApiResponse<object>.Fail($"Export job {jobId} not found."));
        }

        return Ok(ApiResponse<ExportJobResponse>.Ok(job));
    }

    // GET /api/exports/my

    /// <summary>
    /// Get all export jobs for the authenticated user.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<IList<ExportJobResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(CancellationToken ct)
    {
        var jobs = await _exportService.GetExportsByUserAsync(
            User.GetUserId(),
            ct);

        return Ok(ApiResponse<IList<ExportJobResponse>>.Ok(jobs));
    }

    // GET /api/exports/stats

    /// <summary>
    /// Get export statistics for the authenticated user.
    /// Shows total exports by format, today's count, and remaining daily quota.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<ExportStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await _exportService.GetExportStatsAsync(
            User.GetUserId(),
            User.GetSubscriptionPlan(),
            ct);

        return Ok(ApiResponse<ExportStatsResponse>.Ok(stats));
    }

    // DELETE /api/exports/{jobId}

    /// <summary>
    /// Delete an export job record. Only the owner can delete.
    /// </summary>
    [HttpDelete("{jobId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string jobId,
        CancellationToken ct)
    {
        await _exportService.DeleteExportAsync(
            jobId,
            User.GetUserId(),
            ct);

        return NoContent();
    }
}