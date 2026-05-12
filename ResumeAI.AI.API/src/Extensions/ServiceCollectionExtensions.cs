using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResumeAI.AI.BackgroundServices;
using ResumeAI.AI.Configuration;
using ResumeAI.AI.Data;
using ResumeAI.AI.Repositories;
using ResumeAI.AI.Repositories.Interfaces;
using ResumeAI.AI.Services;
using ResumeAI.AI.Services.Interfaces;

namespace ResumeAI.AI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AiDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var secret   = config["Jwt:Secret"]   ?? throw new InvalidOperationException("Jwt:Secret missing.");
        var issuer   = config["Jwt:Issuer"]   ?? "ResumeAI.Auth";
        var audience = config["Jwt:Audience"] ?? "ResumeAI";

        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer           = true,
                    ValidIssuer              = issuer,
                    ValidateAudience         = true,
                    ValidAudience            = audience,
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                };
                o.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode  = 401;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync("""{"success":false,"message":"Authentication required.","data":null}""");
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly",   p => p.RequireRole("ADMIN"))
            .AddPolicy("PremiumOnly", p => p.RequireClaim("subscription_plan", "PREMIUM"));
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration config)
    {
        var redisConn = config.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
            services.AddSingleton<IDistributedCacheIndicator>(_ => new RedisIndicator());
        }
        else
        {
            // Fallback for local dev without Redis
            services.AddDistributedMemoryCache();
        }
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // Bind settings
        services.Configure<AiSettings>(config.GetSection(AiSettings.SectionName));
        services.Configure<ResumeServiceSettings>(config.GetSection(ResumeServiceSettings.SectionName));

        // Repositories
        services.AddScoped<IAiRequestRepository, AiRequestRepository>();

        // Quota service (scoped — uses IDistributedCache)
        services.AddScoped<IQuotaService, QuotaService>();

        // Main AI service
        services.AddScoped<IAiService, AiService>();

        // Background service for monthly quota reset
        services.AddHostedService<QuotaResetBackgroundService>();

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // HTTP client for calling Resume Service to update ATS score
        services.AddHttpClient("ResumeService", (sp, client) =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ResumeServiceSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
        });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "ResumeAI — AI Content Service",
                Version     = "v1",
                Description = "AI-powered content generation, ATS checking, skill suggestions, resume tailoring, and translation."
            });

            var scheme = new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Reference    = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
            };
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });

            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}

// Marker interface to indicate Redis is being used (for logging purposes)
public interface IDistributedCacheIndicator { }
public class RedisIndicator : IDistributedCacheIndicator { }
