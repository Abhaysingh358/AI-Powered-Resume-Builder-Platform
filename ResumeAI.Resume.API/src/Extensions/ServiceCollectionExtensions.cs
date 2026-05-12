using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResumeAI.Resume.Configuration;
using ResumeAI.Resume.Data;
using ResumeAI.Resume.Repositories;
using ResumeAI.Resume.Repositories.Interfaces;
using ResumeAI.Resume.Services;
using ResumeAI.Resume.Services.Interfaces;

namespace ResumeAI.Resume.Extensions;

public static class ServiceCollectionExtensions
{
    //    Database 
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ResumeDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("DefaultConnection")
            )
        );
        return services;
    }

    //    JWT Authentication (validates using same secret as Auth Service) 
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var secret   = config["Jwt:Secret"]   ?? throw new InvalidOperationException("Jwt:Secret is missing.");
        var issuer   = config["Jwt:Issuer"]   ?? "ResumeAI.Auth";
        var audience = config["Jwt:Audience"] ?? "ResumeAI";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
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

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode  = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(
                            """{"success":false,"message":"Authentication required.","data":null}""");
                    }
                };
            });

        return services;
    }

    //    Authorization Policies 
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("PremiumOnly", policy =>
                policy.RequireClaim("subscription_plan", "PREMIUM"))
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole("ADMIN"));

        return services;
    }

    //    Application Services 
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IResumeRepository, ResumeRepository>();
        services.AddScoped<IResumeService,    ResumeService>();
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }

    //    Swagger 
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "ResumeAI — Resume Service",
                Version     = "v1",
                Description = "Resume CRUD, duplication, ATS score management, and public gallery."
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Enter your JWT access token from the Auth Service.",
                Reference    = new OpenApiReference
                {
                    Id   = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });

            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
