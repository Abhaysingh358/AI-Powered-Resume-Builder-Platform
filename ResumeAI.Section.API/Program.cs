using Serilog;
using ResumeAI.Section.Extensions;
using ResumeAI.Section.Middleware;
using ResumeAI.Section.Data;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ResumeAI.Section.API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console());

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
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

    //   COMMENTED OUT — Redis cache 
    // builder.Services.AddStackExchangeRedisCache(options =>
    //     options.Configuration = builder.Configuration.GetConnectionString("Redis"));

    //   COMMENTED OUT — Health Checks 
    // builder.Services.AddHealthChecks()
    //     .AddDbContextCheck<SectionDbContext>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SectionDbContext>();
        Log.Information("Applying EF Core migrations...");
        await db.Database.MigrateAsync();
        Log.Information("Migrations applied successfully.");
    }

    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ResumeAI Section Service v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    //   COMMENTED OUT — Health Check endpoint 
    // app.MapHealthChecks("/health");

    Log.Information("ResumeAI.Section.API started successfully.");
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "ResumeAI.Section.API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
