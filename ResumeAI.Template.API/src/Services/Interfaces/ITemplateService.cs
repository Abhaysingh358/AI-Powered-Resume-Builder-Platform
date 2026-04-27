using ResumeAI.Template.DTOs.Request;
using ResumeAI.Template.DTOs.Response;

namespace ResumeAI.Template.Services.Interfaces;

public interface ITemplateService
{
    // Admin operations
    Task<TemplateResponse>  CreateTemplateAsync(CreateTemplateRequest request, CancellationToken ct = default);
    Task<TemplateResponse>  UpdateTemplateAsync(int templateId, UpdateTemplateRequest request, CancellationToken ct = default);
    Task                    DeactivateTemplateAsync(int templateId, CancellationToken ct = default);

    // Read operations — available to all authenticated users
    Task<TemplateResponse?>           GetTemplateByIdAsync(int templateId, CancellationToken ct = default);
    Task<IList<TemplateListResponse>> GetAllTemplatesAsync(CancellationToken ct = default);
    Task<IList<TemplateListResponse>> GetFreeTemplatesAsync(CancellationToken ct = default);
    Task<IList<TemplateListResponse>> GetPremiumTemplatesAsync(CancellationToken ct = default);
    Task<IList<TemplateListResponse>> GetByCategoryAsync(string category, CancellationToken ct = default);

    // Returns top 10 most-used templates
    Task<IList<TemplateListResponse>> GetPopularTemplatesAsync(CancellationToken ct = default);

    // Called when user selects a template to create a resume
    Task IncrementUsageAsync(int templateId, CancellationToken ct = default);
}
