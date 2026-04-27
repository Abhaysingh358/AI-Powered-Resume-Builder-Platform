using AutoMapper;
using ResumeAI.Template.DTOs.Request;
using ResumeAI.Template.DTOs.Response;
using ResumeAI.Template.Entities;
using ResumeAI.Template.Enums;
using ResumeAI.Template.Repositories.Interfaces;
using ResumeAI.Template.Services.Interfaces;

namespace ResumeAI.Template.Services;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository      _templateRepo;
    private readonly IMapper                  _mapper;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(
        ITemplateRepository templateRepo,
        IMapper mapper,
        ILogger<TemplateService> logger)
    {
        _templateRepo = templateRepo;
        _mapper       = mapper;
        _logger       = logger;
    }

    // ADMIN - Create
    public async Task<TemplateResponse> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken ct = default)
    {
        var category = ParseCategory(request.Category);

        var template = new ResumeTemplate
        {
            Name         = request.Name.Trim(),
            Description  = request.Description?.Trim(),
            ThumbnailUrl = request.ThumbnailUrl?.Trim(),
            HtmlLayout   = request.HtmlLayout,
            CssStyles    = request.CssStyles,
            Category     = category,
            IsPremium    = request.IsPremium,
            IsActive     = true,
            UsageCount   = 0
        };

        var created = await _templateRepo.CreateAsync(template, ct);
        _logger.LogInformation("Template {TemplateId} created: {Name}", created.TemplateId, created.Name);
        return _mapper.Map<TemplateResponse>(created);
    }

    // ADMIN - Update
    public async Task<TemplateResponse> UpdateTemplateAsync(int templateId, UpdateTemplateRequest request, CancellationToken ct = default)
    {
        var template = await RequireTemplateAsync(templateId, ct);

        if (!string.IsNullOrWhiteSpace(request.Name))        template.Name        = request.Name.Trim();
        if (request.Description  is not null)                 template.Description = request.Description.Trim();
        if (request.ThumbnailUrl is not null)                 template.ThumbnailUrl = request.ThumbnailUrl.Trim();
        if (request.HtmlLayout   is not null)                 template.HtmlLayout  = request.HtmlLayout;
        if (request.CssStyles    is not null)                 template.CssStyles   = request.CssStyles;
        if (request.IsPremium    is not null)                 template.IsPremium   = request.IsPremium.Value;

        if (!string.IsNullOrWhiteSpace(request.Category))
            template.Category = ParseCategory(request.Category);

        var updated = await _templateRepo.UpdateAsync(template, ct);
        _logger.LogInformation("Template {TemplateId} updated by admin", templateId);
        return _mapper.Map<TemplateResponse>(updated);
    }

    // ADMIN - Deactivate (soft delete)
    public async Task DeactivateTemplateAsync(int templateId, CancellationToken ct = default)
    {
        var template = await RequireTemplateAsync(templateId, ct);
        template.IsActive = false;
        await _templateRepo.UpdateAsync(template, ct);
        _logger.LogInformation("Template {TemplateId} deactivated by admin", templateId);
    }

    // READ - Get by ID (returns full layout and styles)
    public async Task<TemplateResponse?> GetTemplateByIdAsync(int templateId, CancellationToken ct = default)
    {
        var template = await _templateRepo.FindByTemplateIdAsync(templateId, ct);
        return template is null ? null : _mapper.Map<TemplateResponse>(template);
    }

    // READ - Get all active templates (list view — no layout/styles)
    public async Task<IList<TemplateListResponse>> GetAllTemplatesAsync(CancellationToken ct = default)
    {
        var templates = await _templateRepo.FindByIsActiveAsync(true, ct);
        return _mapper.Map<IList<TemplateListResponse>>(templates);
    }

    // READ - Free templates only
    public async Task<IList<TemplateListResponse>> GetFreeTemplatesAsync(CancellationToken ct = default)
    {
        var templates = await _templateRepo.FindByIsPremiumAsync(false, ct);
        return _mapper.Map<IList<TemplateListResponse>>(templates);
    }

    // READ - Premium templates only
    public async Task<IList<TemplateListResponse>> GetPremiumTemplatesAsync(CancellationToken ct = default)
    {
        var templates = await _templateRepo.FindByIsPremiumAsync(true, ct);
        return _mapper.Map<IList<TemplateListResponse>>(templates);
    }

    // READ - By category
    public async Task<IList<TemplateListResponse>> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        var parsed    = ParseCategory(category);
        var templates = await _templateRepo.FindByCategoryAsync(parsed, ct);
        return _mapper.Map<IList<TemplateListResponse>>(templates);
    }

    // READ - Top 10 most used templates
    public async Task<IList<TemplateListResponse>> GetPopularTemplatesAsync(CancellationToken ct = default)
    {
        var templates = await _templateRepo.FindAllOrderByUsageCountDescAsync(ct);
        return _mapper.Map<IList<TemplateListResponse>>(templates.Take(10).ToList());
    }

    // Called when user picks a template to create a resume
    public async Task IncrementUsageAsync(int templateId, CancellationToken ct = default)
    {
        var exists = await _templateRepo.FindByTemplateIdAsync(templateId, ct);
        if (exists is null || !exists.IsActive)
            throw new KeyNotFoundException($"Template {templateId} not found or inactive.");

        await _templateRepo.UpdateUsageCountAsync(templateId, ct);
        _logger.LogInformation("Usage incremented for template {TemplateId}", templateId);
    }

    // Helpers
    private async Task<ResumeTemplate> RequireTemplateAsync(int templateId, CancellationToken ct)
        => await _templateRepo.FindByTemplateIdAsync(templateId, ct)
           ?? throw new KeyNotFoundException($"Template {templateId} not found.");

    private static TemplateCategory ParseCategory(string category)
    {
        if (!Enum.TryParse<TemplateCategory>(category, ignoreCase: true, out var parsed))
            throw new ArgumentException(
                $"Invalid category '{category}'. Valid values: {string.Join(", ", Enum.GetNames<TemplateCategory>())}");
        return parsed;
    }
}
