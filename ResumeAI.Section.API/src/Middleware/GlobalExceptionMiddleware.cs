using System.Net;
using System.Text.Json;
using ResumeAI.Section.DTOs.Response;

namespace ResumeAI.Section.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            KeyNotFoundException        => (HttpStatusCode.NotFound,            ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden,           ex.Message),
            InvalidOperationException   => (HttpStatusCode.Conflict,            ex.Message),
            ArgumentException           => (HttpStatusCode.BadRequest,          ex.Message),
            _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var body = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(body,
            new JsonSerializerOptions { PropertyNamingPolicy = null });

        await context.Response.WriteAsync(json);
    }
}
