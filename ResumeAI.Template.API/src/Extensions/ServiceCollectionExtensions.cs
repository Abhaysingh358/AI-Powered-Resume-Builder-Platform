using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResumeAI.Template.Configuration;
using ResumeAI.Template.Data;
using ResumeAI.Template.Repositories;
using ResumeAI.Template.Repositories.Interfaces;
using ResumeAI.Template.Services;
using ResumeAI.Template.Services.Interfaces;

namespace ResumeAI.Template.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<TemplateDbContext>(options =>
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
                        return ctx.Response.WriteAsync(
                            """{"success":false,"message":"Authentication required.","data":null}""");
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

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<ITemplateService,    TemplateService>();
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "ResumeAI — Template Service",
                Version     = "v1",
                Description = "Resume template library management. Browse, filter, and select templates."
            });

            var scheme = new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Reference    = new OpenApiReference
                {
                    Id   = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { scheme, Array.Empty<string>() }
            });

            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
