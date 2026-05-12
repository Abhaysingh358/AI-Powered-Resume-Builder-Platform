using Serilog;
using ResumeAI.Auth.Extensions;
using ResumeAI.Auth.Middleware;
using ResumeAI.Auth.Data;
using Microsoft.EntityFrameworkCore;

// ------------------ Bootstrap Serilog immediately so startup errors are captured --------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ResumeAI.Auth.API...");

    var builder = WebApplication.CreateBuilder(args);

    // -------------- Serilog (reads config from appsettings.json Serilog section) ----------------
    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console());

    // -------------------------- Controllers -------------------------------
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Keep PascalCase on the wire — change to CamelCase if preferred
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

    // ------------------- Database (PostgreSQL via EF Core) --------------------------------
    builder.Services.AddDatabase(builder.Configuration);

    // ---- Application Services (Repositories, Services, AutoMapper, etc.) 
    builder.Services.AddApplicationServices();

    // ---- JWT Authentication + Google OAuth -----------------------------
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // ---- Authorization Policies (AdminOnly, PremiumOnly) -----------------------
    builder.Services.AddAuthorizationPolicies();

    // ---- Swagger / OpenAPI -------------------------------
    builder.Services.AddSwagger();

    // ---- CORS -----------------------------
    // Currently allows all origins — tighten in production by replacing
    // AllowAnyOrigin() with WithOrigins("https://yourfrontend.com")
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // ---- COMMENTED OUT — Redis distributed cache---------------------
    // builder.Services.AddStackExchangeRedisCache(options =>
    //     options.Configuration = builder.Configuration.GetConnectionString("Redis"));

    // ---- COMMENTED OUT — Health Checks --------------------------
    // builder.Services.AddHealthChecks()
    //     .AddDbContextCheck<AuthDbContext>();

    // --------------------------------------------------------------------
    var app = builder.Build();
    // ------------------------------------------------------------------

    // ---- Auto-run EF Core migrations on startup -----------------------
    // Convenient for development — in production use: dotnet ef database update
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        Log.Information("Applying EF Core migrations...");
        await db.Database.MigrateAsync();
        Log.Information("Migrations applied successfully.");
    }

    // ---- Global Exception Handler (must be first middleware) ----------------------
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // ---- Swagger UI (Development only) ----------------------------------------
    // if (app.Environment.IsDevelopment())
    // {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ResumeAI Auth Service v1");
            c.RoutePrefix = string.Empty; // Swagger opens at root: http://localhost:5104
        });
    // }

    // ---- Request Logging (Serilog) -------------------------------------
    app.UseSerilogRequestLogging();

    // ---- HTTPS Redirection--------------------------------------------
    app.UseHttpsRedirection();

    // ---- CORS (must be before Auth) -------------------------------------
    app.UseCors("AllowAll");

    // ---- Authentication & Authorization------------------------------------
    app.UseAuthentication();
    app.UseAuthorization();

    // ---- Controllers -------------------------------------------------------
    app.MapControllers();

    // ---- COMMENTED OUT — Health Check endpoint ---------------------------------------
    // Uncomment together with AddHealthChecks() above when needed
    // app.MapHealthChecks("/health");

    Log.Information("ResumeAI.Auth.API started successfully.");

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "ResumeAI.Auth.API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

// Required for integration test WebApplicationFactory<Program>
// public partial class Program { }
