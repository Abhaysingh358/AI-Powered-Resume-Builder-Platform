using System.ComponentModel.DataAnnotations;

namespace ResumeAI.Template.DTOs.Request;

public record CreateTemplateRequest
{
    [Required][MaxLength(100)] 
    public string Name        { get; init; } = string.Empty;
    [MaxLength(500)]           
    public string? Description { get; init; }
    [MaxLength(500)]           
    public string? ThumbnailUrl { get; init; }
    public string? HtmlLayout  { get; init; }
    public string? CssStyles   { get; init; }
    [Required]                 
    public string Category    { get; init; } = string.Empty;
    public bool    IsPremium   { get; init; } = false;
}

public record UpdateTemplateRequest
{
    [MaxLength(100)] 
    public string? Name   { get; init; }
    [MaxLength(500)] 
    public string? Description  { get; init; }
    [MaxLength(500)] 
    public string? ThumbnailUrl { get; init; }
    public string?   HtmlLayout  { get; init; }
    public string?   CssStyles   { get; init; }
    public string?   Category    { get; init; }
    public bool?     IsPremium   { get; init; }
}
