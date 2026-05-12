using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResumeAI.Export.BackgroundServices;
using ResumeAI.Export.Configuration;
using ResumeAI.Export.Data;
using ResumeAI.Export.Repositories;
using ResumeAI.Export.Repositories.Interfaces;
using ResumeAI.Export.Services;
using ResumeAI.Export.Services.Interfaces;

namespace ResumeAI.Export.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<ExportDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        var secret =
            config["Jwt:Secret"] ??
            throw new InvalidOperationException("Jwt:Secret missing.");

        var issuer = config["Jwt:Issuer"] ?? "ResumeAI.Auth";
        var audience = config["Jwt:Audience"] ?? "ResumeAI";

        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                o.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";

                        return ctx.Response.WriteAsync(
                            """{"success":false,"message":"Authentication required.","data":null}"""
                        );
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"))
            .AddPolicy(
                "PremiumOnly",
                p => p.RequireClaim("subscription_plan", "PREMIUM")
            );

        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IExportRepository, ExportRepository>();
        services.AddScoped<IPdfRenderer, QuestPdfRenderer>();
        services.AddScoped<IDocxRenderer, OpenXmlDocxRenderer>();
        services.AddScoped<IExportService, ExportService>();

        services.AddHostedService<ExportCleanupBackgroundService>();

        services.AddAutoMapper(typeof(MappingProfile));

        return services;
    }

    // MassTransit with RabbitMQ — falls back to in-memory if RabbitMQ is disabled
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration config)
    {
        var rabbitHost = config["RabbitMQ:Host"];
        var rabbitUser = config["RabbitMQ:Username"];
        var rabbitPassword = config["RabbitMQ:Password"];
        var rabbitVHost = config["RabbitMQ:VHost"];
        var useRabbitMQ = config.GetValue<bool>("RabbitMQ:Enabled");

        services.AddMassTransit(x =>
        {
            if (useRabbitMQ)
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    // host = hostname only (e.g. hawk.rmq.cloudamqp.com)
                    // port = 5671 (SSL required for CloudAMQP)
                    // vhost = your CloudAMQP vhost

                    cfg.Host(rabbitHost, 5671, rabbitVHost, h =>
                    {
                        h.Username(rabbitUser);
                        h.Password(rabbitPassword);

                        // REQUIRED for CloudAMQP
                        h.UseSsl(s =>
                        {
                            s.Protocol =
                                System.Security.Authentication.SslProtocols.Tls12;
                        });
                    });

                    // Retry 3x with 5s gap
                    cfg.UseMessageRetry(r =>
                        r.Interval(3, TimeSpan.FromSeconds(5)));

                    cfg.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                // In-memory fallback — local dev only
                x.UsingInMemory((ctx, cfg) =>
                {
                    cfg.ConfigureEndpoints(ctx);
                });

                /*
                // OLD LOCAL RABBITMQ (COMMENTED — DO NOT DELETE)
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ConfigureEndpoints(ctx);
                });
                */
            }
        });

        return services;
    }

    public static IServiceCollection AddSwagger(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "ResumeAI — Export Service",
                    Version = "v1",
                    Description =
                        "Resume export to PDF (QuestPDF), DOCX (OpenXML), and JSON. Enforces FREE tier daily limit of 10 PDF exports."
                }
            );

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(
                JwtBearerDefaults.AuthenticationScheme,
                scheme
            );

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    { scheme, Array.Empty<string>() }
                }
            );

            var xmlFile =
                $"{typeof(Program).Assembly.GetName().Name}.xml";

            var xmlPath =
                Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}