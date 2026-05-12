using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Ganss.Xss;
using Microsoft.Extensions.Options;
using ResumeAI.AI.Configuration;
using ResumeAI.AI.DTOs.Request;
using ResumeAI.AI.DTOs.Response;
using ResumeAI.AI.Entities;
using ResumeAI.AI.Enums;
using ResumeAI.AI.Repositories.Interfaces;
using ResumeAI.AI.Services.Interfaces;

namespace ResumeAI.AI.Services;

public class AiService : IAiService
{
    private readonly IAiRequestRepository _aiRepo;
    private readonly IQuotaService        _quota;
    private readonly AiSettings           _settings;
    private readonly IHttpClientFactory   _httpClientFactory;
    private readonly HtmlSanitizer        _sanitizer;
    private readonly ILogger<AiService>   _logger;

    public AiService(
        IAiRequestRepository aiRepo,
        IQuotaService quota,
        IOptions<AiSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<AiService> logger)
    {
        _aiRepo            = aiRepo;
        _quota             = quota;
        _settings          = settings.Value;
        _httpClientFactory = httpClientFactory;
        _sanitizer         = new HtmlSanitizer();
        _logger            = logger;
    }

    public async Task<string> GenerateSummaryAsync(
        int userId, string subscriptionPlan, GenerateSummaryRequest request, CancellationToken ct = default)
    {
        await EnforceQuotaAsync(userId, subscriptionPlan, RequestType.SUMMARY, ct);
        var prompt = BuildSummaryPrompt(Sanitize(request.JobTitle), Sanitize(request.KeySkills), request.YearsExperience);
        var (response, model, tokens) = await CallAiAsync(prompt, ct);
        await PersistAndIncrementAsync(userId, request.ResumeId, RequestType.SUMMARY, prompt, response, model, tokens, ct);
        return response;
    }

    public async Task<IList<string>> GenerateBulletPointsAsync(
        int userId, string subscriptionPlan, GenerateBulletsRequest request, CancellationToken ct = default)
    {
        await EnforceQuotaAsync(userId, subscriptionPlan, RequestType.BULLETS, ct);
        var prompt = BuildBulletsPrompt(Sanitize(request.JobTitle), Sanitize(request.Responsibilities), Sanitize(request.CompanyName ?? ""));
        var (response, model, tokens) = await CallAiAsync(prompt, ct);
        await PersistAndIncrementAsync(userId, request.ResumeId, RequestType.BULLETS, prompt, response, model, tokens, ct);
        var bullets = response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                              .Select(b => b.TrimStart('-', '*', ' ').Trim())
                              .Where(b => !string.IsNullOrWhiteSpace(b))
                              .ToList();
        return bullets;
    }

    public async Task<string> GenerateCoverLetterAsync(
        int userId, string subscriptionPlan, GenerateCoverLetterRequest request, CancellationToken ct = default)
    {
        await EnforceQuotaAsync(userId, subscriptionPlan, RequestType.COVER_LETTER, ct);
        var prompt = BuildCoverLetterPrompt(Sanitize(request.ApplicantName), Sanitize(request.JobDescription), Sanitize(request.CompanyName ?? "the company"));
        var (response, model, tokens) = await CallAiAsync(prompt, ct);
        await PersistAndIncrementAsync(userId, request.ResumeId, RequestType.COVER_LETTER, prompt, response, model, tokens, ct);
        return response;
    }

    public async Task<string> ImproveSectionAsync(
        int userId, string subscriptionPlan, ImproveSectionRequest request, CancellationToken ct = default)
    {
        await EnforceQuotaAsync(userId, subscriptionPlan, RequestType.IMPROVE, ct);
        var prompt = BuildImproveSectionPrompt(Sanitize(request.SectionType), Sanitize(request.CurrentContent));
        var (response, model, tokens) = await CallAiAsync(prompt, ct);
        await PersistAndIncrementAsync(userId, request.ResumeId, RequestType.IMPROVE, prompt, response, model, tokens, ct);
        return response;
    }

    public async Task<AtsReportResponse> CheckAtsCompatibilityAsync(
        int userId, string subscriptionPlan, CheckAtsRequest request, CancellationToken ct = default)
    {
        await EnforceQuotaAsync(userId, subscriptionPlan, RequestType.ATS, ct);
        var prompt = BuildAtsPrompt(Sanitize(request.ResumeText), Sanitize(request.JobDescription));
        var (response, model, tokens) = await CallAiAsync(prompt, ct);
        await PersistAndIncrementAsync(userId, request.ResumeId, RequestType.ATS, prompt, response, model, tokens, ct);
        var report = ParseAtsResponse(response);
        await UpdateResumeAtsScoreAsync(request.ResumeId, report.Score, ct);
        return report;
    }

    public async Task<IList<string>> SuggestSkillsAsync(
        int userId, string subscriptionPlan, SuggestSkillsRequest request, CancellationToken ct = default)
    {
        await EnforceQuotaAsync(userId, subscriptionPlan, RequestType.SKILLS, ct);
        var prompt = BuildSkillsPrompt(Sanitize(request.JobTitle));
        var (response, model, tokens) = await CallAiAsync(prompt, ct);
        await PersistAndIncrementAsync(userId, request.ResumeId, RequestType.SKILLS, prompt, response, model, tokens, ct);
        var skills = response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => s.TrimStart('-', '*', ' ').Trim())
                             .Where(s => !string.IsNullOrWhiteSpace(s))
                             .ToList();
        return skills;
    }

    public async Task<string> TailorResumeForJobAsync(
        int userId, string subscriptionPlan, TailorResumeRequest request, CancellationToken ct = default)
    {
        await EnforceQuotaAsync(userId, subscriptionPlan, RequestType.TAILOR, ct);
        var prompt = BuildTailorPrompt(Sanitize(request.ResumeJson), Sanitize(request.JobDescription));
        var (response, model, tokens) = await CallAiAsync(prompt, ct);
        await PersistAndIncrementAsync(userId, request.ResumeId, RequestType.TAILOR, prompt, response, model, tokens, ct);
        return response;
    }

    public async Task<string> TranslateResumeAsync(
        int userId, string subscriptionPlan, TranslateResumeRequest request, CancellationToken ct = default)
    {
        await EnforceQuotaAsync(userId, subscriptionPlan, RequestType.TRANSLATE, ct);
        var prompt = BuildTranslatePrompt(Sanitize(request.ResumeContent), Sanitize(request.TargetLanguage));
        var (response, model, tokens) = await CallAiAsync(prompt, ct);
        await PersistAndIncrementAsync(userId, request.ResumeId, RequestType.TRANSLATE, prompt, response, model, tokens, ct);
        return response;
    }

    public async Task<IList<AiRequest>> GetAiHistoryAsync(int userId, CancellationToken ct = default)
        => await _aiRepo.FindByUserIdAsync(userId, ct);

    public async Task<QuotaResponse> GetRemainingQuotaAsync(int userId, string subscriptionPlan, CancellationToken ct = default)
    {
        var isPremium = subscriptionPlan.Equals("PREMIUM", StringComparison.OrdinalIgnoreCase);

        if (isPremium)
        {
            return new QuotaResponse
            {
                IsPremium          = true,
                AiCallsUsed        = await _quota.GetUsedCountAsync(userId, RequestType.SUMMARY, ct),
                AiCallsLimit       = -1,
                AtsChecksUsed      = await _quota.GetUsedCountAsync(userId, RequestType.ATS, ct),
                AtsChecksLimit     = -1,
                AiCallsRemaining   = -1,
                AtsChecksRemaining = -1,
                ResetDate          = GetNextResetDate()
            };
        }

        var aiUsed  = await _quota.GetUsedCountAsync(userId, RequestType.SUMMARY, ct);
        var atsUsed = await _quota.GetUsedCountAsync(userId, RequestType.ATS, ct);

        return new QuotaResponse
        {
            IsPremium          = false,
            AiCallsUsed        = aiUsed,
            AiCallsLimit       = _settings.FreeMonthlyAiCallLimit,
            AtsChecksUsed      = atsUsed,
            AtsChecksLimit     = _settings.FreeMonthlyAtsCallLimit,
            AiCallsRemaining   = Math.Max(0, _settings.FreeMonthlyAiCallLimit  - aiUsed),
            AtsChecksRemaining = Math.Max(0, _settings.FreeMonthlyAtsCallLimit - atsUsed),
            ResetDate          = GetNextResetDate()
        };
    }

    // CALL AI - Groq primary, OpenAI fallback, Claude second fallback
    private async Task<(string Response, AiModel Model, int Tokens)> CallAiAsync(
        string prompt, CancellationToken ct)
    {
        // PRIMARY - Groq (free, fast)
        if (!string.IsNullOrWhiteSpace(_settings.GroqApiKey))
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.GroqApiKey}");

                var requestBody = new
                {
                    model      = _settings.GroqModel,
                    messages   = new[] { new { role = "user", content = prompt } },
                    max_tokens = 1024
                };

                var json     = JsonSerializer.Serialize(requestBody);
                var content  = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("chat/completions", content, ct);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(ct);
                var doc          = JsonDocument.Parse(responseJson);
                var text         = doc.RootElement
                                      .GetProperty("choices")[0]
                                      .GetProperty("message")
                                      .GetProperty("content")
                                      .GetString() ?? string.Empty;

                var tokens = doc.RootElement
                                .GetProperty("usage")
                                .GetProperty("total_tokens")
                                .GetInt32();

                _logger.LogInformation("Groq response received. Tokens used: {Tokens}", tokens);
                return (text, AiModel.GPT4O, tokens);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Groq call failed — falling back to OpenAI.");
            }
        }

        // FALLBACK 1 - OpenAI GPT-4o (paid)
        if (!string.IsNullOrWhiteSpace(_settings.OpenAiApiKey))
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.OpenAiApiKey}");

                var requestBody = new
                {
                    model      = _settings.OpenAiModel,
                    messages   = new[] { new { role = "user", content = prompt } },
                    max_tokens = 1024
                };

                var json     = JsonSerializer.Serialize(requestBody);
                var content  = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("chat/completions", content, ct);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(ct);
                var doc          = JsonDocument.Parse(responseJson);
                var text         = doc.RootElement
                                      .GetProperty("choices")[0]
                                      .GetProperty("message")
                                      .GetProperty("content")
                                      .GetString() ?? string.Empty;

                var tokens = doc.RootElement
                                .GetProperty("usage")
                                .GetProperty("total_tokens")
                                .GetInt32();

                _logger.LogInformation("OpenAI response received. Tokens used: {Tokens}", tokens);
                return (text, AiModel.GPT4O, tokens);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI call failed — falling back to Claude.");
            }
        }

        // FALLBACK 2 - Anthropic Claude
        if (!string.IsNullOrWhiteSpace(_settings.AnthropicApiKey))
        {
            try
            {
                var client = new AnthropicClient(_settings.AnthropicApiKey);
                var messageRequest = new MessageParameters
                {
                    Model     = _settings.ClaudeModel,
                    MaxTokens = 1024,
                    Messages  = new List<Message>
                    {
                        new Message
                        {
                            Role    = RoleType.User,
                            Content = new List<ContentBase>
                            {
                                new TextContent { Text = prompt }
                            }
                        }
                    }
                };
                var result = await client.Messages.GetClaudeMessageAsync(messageRequest, ct);
                var text   = result.Content[0].ToString() ?? string.Empty;
                var tokens = result.Usage.InputTokens + result.Usage.OutputTokens;
                _logger.LogInformation("Claude response received. Tokens used: {Tokens}", tokens);
                return (text, AiModel.CLAUDE, tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "All AI providers failed.");
                throw new InvalidOperationException("All AI providers failed. Please try again later.");
            }
        }

        throw new InvalidOperationException(
            "No AI API key configured. Add GroqApiKey or OpenAiApiKey in appsettings.Development.json.");
    }

    // ENFORCE QUOTA
    private async Task EnforceQuotaAsync(int userId, string subscriptionPlan, RequestType type, CancellationToken ct)
    {
        if (subscriptionPlan.Equals("PREMIUM", StringComparison.OrdinalIgnoreCase))
            return;

        var isAts  = _quota.IsAtsRequest(type);
        var limit  = isAts ? _settings.FreeMonthlyAtsCallLimit : _settings.FreeMonthlyAiCallLimit;
        var key    = isAts ? RequestType.ATS : RequestType.SUMMARY;
        var used   = await _quota.GetUsedCountAsync(userId, key, ct);

        if (used >= limit)
        {
            var typeName = isAts ? "ATS check" : "AI call";
            throw new InvalidOperationException(
                $"You have reached your monthly {typeName} limit of {limit}. " +
                "Upgrade to Premium for unlimited access.");
        }
    }

    // PERSIST REQUEST AND INCREMENT QUOTA
    private async Task PersistAndIncrementAsync(
        int userId, int resumeId, RequestType type,
        string prompt, string response, AiModel model, int tokens,
        CancellationToken ct)
    {
        await _aiRepo.CreateAsync(new AiRequest
        {
            RequestId   = Guid.NewGuid().ToString(),
            UserId      = userId,
            ResumeId    = resumeId,
            RequestType = type,
            InputPrompt = prompt,
            AiResponse  = response,
            Model       = model,
            TokensUsed  = tokens,
            Status      = RequestStatus.COMPLETED,
            CompletedAt = DateTime.UtcNow
        }, ct);

        var quotaKey = _quota.IsAtsRequest(type) ? RequestType.ATS : RequestType.SUMMARY;
        await _quota.IncrementAsync(userId, quotaKey, ct);
    }

    // UPDATE ATS SCORE IN RESUME SERVICE VIA HTTP
    private async Task UpdateResumeAtsScoreAsync(int resumeId, int score, CancellationToken ct)
    {
        try
        {
            var client  = _httpClientFactory.CreateClient("ResumeService");
            var body    = JsonSerializer.Serialize(new { AtsScore = score });
            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var res     = await client.PutAsync($"/api/resumes/{resumeId}/ats-score", content, ct);

            if (!res.IsSuccessStatusCode)
                _logger.LogWarning("ATS score update returned {Status} for resume {ResumeId}", res.StatusCode, resumeId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update ATS score in Resume Service for resume {ResumeId}", resumeId);
        }
    }

    // SANITIZE USER INPUT to prevent prompt injection
    private string Sanitize(string input) => _sanitizer.Sanitize(input);

    private static string GetNextResetDate()
    {
        var next = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);
        return next.ToString("yyyy-MM-dd");
    }

    // PARSE ATS RESPONSE
    private static AtsReportResponse ParseAtsResponse(string response)
    {
        var lines          = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var score          = 0;
        var missing        = new List<string>();
        var recommendation = string.Empty;

        foreach (var line in lines)
        {
            if (line.StartsWith("SCORE:", StringComparison.OrdinalIgnoreCase))
                int.TryParse(line.Replace("SCORE:", "").Trim(), out score);
            else if (line.StartsWith("MISSING:", StringComparison.OrdinalIgnoreCase))
                missing = line.Replace("MISSING:", "").Split(',').Select(k => k.Trim()).ToList();
            else if (line.StartsWith("RECOMMENDATION:", StringComparison.OrdinalIgnoreCase))
                recommendation = line.Replace("RECOMMENDATION:", "").Trim();
        }

        return new AtsReportResponse
        {
            Score           = Math.Clamp(score, 0, 100),
            MissingKeywords = missing,
            Recommendation  = recommendation
        };
    }

    // PROMPT BUILDERS
    private static string BuildSummaryPrompt(string jobTitle, string skills, int years) =>
        $"Write a professional resume summary for a {jobTitle} with {years} years of experience. " +
        $"Key skills include: {skills}. " +
        "Keep it 3-4 sentences, professional tone, ATS-friendly. Return only the summary text.";

    private static string BuildBulletsPrompt(string jobTitle, string responsibilities, string company) =>
        $"Generate 5 impactful resume bullet points for a {jobTitle}{(string.IsNullOrWhiteSpace(company) ? "" : $" at {company}")}. " +
        $"Responsibilities: {responsibilities}. " +
        "Start each bullet with a strong action verb. Use quantifiable achievements where possible. " +
        "Return one bullet per line, no numbering.";

    private static string BuildCoverLetterPrompt(string name, string jobDescription, string company) =>
        $"Write a professional cover letter for {name} applying to {company}. " +
        $"Job description: {jobDescription}. " +
        "Keep it concise (3 paragraphs), professional tone. Return only the cover letter text.";

    private static string BuildImproveSectionPrompt(string sectionType, string content) =>
        $"Improve the following resume {sectionType} section for a more impactful professional tone. " +
        $"Original content: {content}. " +
        "Keep the same information but make it more concise and impactful. Return only the improved text.";

    private static string BuildAtsPrompt(string resumeText, string jobDescription) =>
        $"Analyze this resume against the job description for ATS compatibility. " +
        $"Resume: {resumeText}\n\nJob Description: {jobDescription}\n\n" +
        "Respond in exactly this format:\n" +
        "SCORE: [number 0-100]\n" +
        "MISSING: [comma separated missing keywords]\n" +
        "RECOMMENDATION: [one sentence recommendation]";

    private static string BuildSkillsPrompt(string jobTitle) =>
        $"List 10 relevant technical and soft skills for a {jobTitle} resume. " +
        "Return one skill per line, no numbering, no explanations.";

    private static string BuildTailorPrompt(string resumeJson, string jobDescription) =>
        $"Tailor this resume JSON to better match the job description. " +
        $"Resume JSON: {resumeJson}\n\nJob Description: {jobDescription}\n\n" +
        "Return only the updated resume JSON with improved content matching the job requirements.";

    private static string BuildTranslatePrompt(string content, string targetLanguage) =>
        $"Translate the following resume content to {targetLanguage}. " +
        "Maintain professional tone and formatting. " +
        $"Content: {content}\n\nReturn only the translated text.";
}