using ResumeAI.Export.Entities;
using ResumeAI.Export.Enums;

namespace ResumeAI.Export.Repositories.Interfaces;

public interface IExportRepository
{
    Task<ExportJob?>   FindByJobIdAsync(string jobId, CancellationToken ct = default);
    Task<IList<ExportJob>> FindByUserIdAsync(int userId, CancellationToken ct = default);
    Task<IList<ExportJob>> FindByResumeIdAsync(int resumeId, CancellationToken ct = default);
    Task<IList<ExportJob>> FindByStatusAsync(ExportStatus status, CancellationToken ct = default);
    Task<IList<ExportJob>> FindByFormatAsync(ExportFormat format, CancellationToken ct = default);
    Task<IList<ExportJob>> FindExpiredJobsAsync(DateTime cutoff, CancellationToken ct = default);
    Task<int>  CountByUserIdTodayAsync(int userId, CancellationToken ct = default);
    Task<int>  CountPdfByUserIdTodayAsync(int userId, CancellationToken ct = default);

    Task<ExportJob> CreateAsync(ExportJob job, CancellationToken ct = default);
    Task<ExportJob> UpdateAsync(ExportJob job, CancellationToken ct = default);
    Task  DeleteByJobIdAsync(string jobId, CancellationToken ct = default);
    Task  DeleteExpiredAsync(CancellationToken ct = default);
}
