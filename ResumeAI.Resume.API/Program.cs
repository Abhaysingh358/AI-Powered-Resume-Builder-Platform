using Serilog;
using ResumeAI.Resume.Extensions;
using ResumeAI.Resume.Middleware;
using ResumeAI.Resume.Data;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ResumeAI.Resume.API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console());

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });

    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthorizationPolicies();
    builder.Services.AddSwagger();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // ── COMMENTED OUT — Redis cache ───────────────────────────────────────────
    // Uncomment when adding caching for public gallery or resume lookups
    // builder.Services.AddStackExchangeRedisCache(options =>
    //     options.Configuration = builder.Configuration.GetConnectionString("Redis"));

    // ── COMMENTED OUT — Health Checks ─────────────────────────────────────────
    // Uncomment when deploying to Kubernetes / Docker with uptime monitoring
    // builder.Services.AddHealthChecks()
    //     .AddDbContextCheck<ResumeDbContext>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ResumeDbContext>();
        Log.Information("Applying EF Core migrations...");
        await db.Database.MigrateAsync();
        Log.Information("Migrations applied successfully.");
    }

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseCors("AllowAll");
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ResumeAI Resume Service v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseSerilogRequestLogging();
    // app.UseHttpsRedirection();
    
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ── COMMENTED OUT — Health Check endpoint ─────────────────────────────────
    // app.MapHealthChecks("/health");

    Log.Information("ResumeAI.Resume.API started successfully.");
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "ResumeAI.Resume.API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
