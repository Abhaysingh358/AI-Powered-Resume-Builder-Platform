using ResumeAI.Export.DTOs.Request;
using ResumeAI.Export.DTOs.Response;
using ResumeAI.Export.Entities;

namespace ResumeAI.Export.Services.Interfaces;

public interface IExportService
{
    // Export submission — generates file immediately and returns bytes + job record
    Task<(byte[] FileBytes, ExportJob Job)> ExportToPdfAsync(int userId, string subscriptionPlan, ExportPdfRequest request, CancellationToken ct = default);
    Task<(byte[] FileBytes, ExportJob Job)> ExportToDocxAsync(int userId, ExportDocxRequest request, CancellationToken ct = default);
    Task<(byte[] FileBytes, ExportJob Job)> ExportToJsonAsync(int userId, ExportJsonRequest request, CancellationToken ct = default);

    // Job tracking
    Task<ExportJobResponse?>       GetJobStatusAsync(string jobId, CancellationToken ct = default);
    Task<IList<ExportJobResponse>> GetExportsByUserAsync(int userId, CancellationToken ct = default);

    // Download by jobId — returns file bytes from in-memory regeneration
    Task<(byte[] FileBytes, string FileName, string ContentType)?> DownloadFileAsync(string jobId, int userId, CancellationToken ct = default);

    // Deletion
    Task DeleteExportAsync(string jobId, int userId, CancellationToken ct = default);

    // Called by IHostedService daily — deletes expired job records
    Task CleanupExpiredExportsAsync(CancellationToken ct = default);

    // Stats for the authenticated user
    Task<ExportStatsResponse> GetExportStatsAsync(int userId, string subscriptionPlan, CancellationToken ct = default);
}
