namespace ResumeAI.AI.Configuration;

public class AiSettings
{
    public const string SectionName = "AiSettings";

    // Groq settings (primary — free)
    public string GroqApiKey  { get; set; } = string.Empty;
    public string GroqModel   { get; set; } = "llama-3.3-70b-versatile";

    // OpenAI settings (fallback — paid)
    public string OpenAiApiKey  { get; set; } = string.Empty;
    public string OpenAiModel   { get; set; } = "gpt-4o";

    // Anthropic Claude settings (second fallback)
    public string AnthropicApiKey { get; set; } = string.Empty;
    public string ClaudeModel     { get; set; } = "claude-sonnet-4-6";

    // Quota limits
    public int FreeMonthlyAiCallLimit  { get; set; } = 5;
    public int FreeMonthlyAtsCallLimit { get; set; } = 3;
}

public class ResumeServiceSettings
{
    public const string SectionName = "ResumeService";
    public string BaseUrl { get; set; } = "http://localhost:5105";
}