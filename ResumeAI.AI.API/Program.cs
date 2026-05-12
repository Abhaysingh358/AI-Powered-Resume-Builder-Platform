using Serilog;
using ResumeAI.AI.Extensions;
using ResumeAI.AI.Middleware;
using ResumeAI.AI.Data;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ResumeAI.AI.API...");

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
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthorizationPolicies();
    builder.Services.AddRedisCache(builder.Configuration);
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddSwagger();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AiDbContext>();
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ResumeAI AI Service v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("ResumeAI.AI.API started successfully.");
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "ResumeAI.AI.API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
