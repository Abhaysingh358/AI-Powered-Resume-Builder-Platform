using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResumeAI.Section.Configuration;
using ResumeAI.Section.Data;
using ResumeAI.Section.Repositories;
using ResumeAI.Section.Repositories.Interfaces;
using ResumeAI.Section.Services;
using ResumeAI.Section.Services.Interfaces;

namespace ResumeAI.Section.Extensions;

public static class ServiceCollectionExtensions
{
    //   Database 
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<SectionDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        return services;
    }

    //   JWT Authentication 
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

    //   Authorization Policies 
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly",   policy => policy.RequireRole("ADMIN"))
            .AddPolicy("PremiumOnly", policy => policy.RequireClaim("subscription_plan", "PREMIUM"));
        return services;
    }

    //   Application Services 
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<ISectionService,    SectionService>();
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }

    //   Swagger 
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "ResumeAI — Section Service",
                Version     = "v1",
                Description = "Resume section CRUD, drag-and-drop reordering, bulk update, and visibility toggle."
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
            if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
