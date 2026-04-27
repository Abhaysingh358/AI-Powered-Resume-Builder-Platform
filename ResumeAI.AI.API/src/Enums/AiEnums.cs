namespace ResumeAI.AI.Enums;

public enum RequestType
{
    SUMMARY,
    BULLETS,
    COVER_LETTER,
    IMPROVE,
    ATS,
    SKILLS,
    TAILOR,
    TRANSLATE
}

public enum AiModel
{
    GPT4O,
    CLAUDE
}

public enum RequestStatus
{
    QUEUED,
    COMPLETED,
    FAILED
}
