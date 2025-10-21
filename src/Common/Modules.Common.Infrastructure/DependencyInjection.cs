using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Modules.Common.Infrastructure.Configuration;
using Modules.Common.Infrastructure.Policies;
using Npgsql; // For Npgsql OpenTelemetry
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using Microsoft.Extensions.DependencyInjection;


// Put extensions into the global Microsoft namespace for easy discovery
// ReSharper disable once CheckNamespace
namespace Modules.Common.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers core infrastructure services shared across all modules.
    /// </summary>
    public static IServiceCollection AddCoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string[] activityModuleNames) // Module names for OpenTelemetry tracing
    {
        services.AddMemoryCache(); // Needed for token revocation middleware

        services.AddHostOpenTelemetry(activityModuleNames);

        services.AddJwtAuthentication(configuration);
        services.AddClaimsAuthorization(); // Sets up the policy discovery system

        // Register the AuditableInterceptor as a singleton
        services.AddSingleton<Database.AuditableInterceptor>();


        return services;
    }

    private static IServiceCollection AddHostOpenTelemetry(
        this IServiceCollection services,
        params string[] activityModuleNames)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("BooksApp")) // Service name for tracing
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation() // Traces incoming/outgoing HTTP requests
                    .AddHttpClientInstrumentation()  // Traces outgoing HttpClient calls
                    .AddNpgsql()                   // Traces EF Core PostgreSQL commands
                    .AddSource(activityModuleNames); // Adds sources from our modules

                // Configure exporter (e.g., OTLP for Jaeger/Aspire)
                // We'll read the endpoint from configuration later
                tracing.AddOtlpExporter();
            });

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind the "AuthConfiguration" section from appsettings.json to the AuthConfiguration record
        services.AddOptions<AuthConfiguration>()
            .Bind(configuration.GetSection(nameof(AuthConfiguration)))
            .ValidateDataAnnotations() // Optional: Add validation if you use data annotations
            .ValidateOnStart();       // Validate settings on application startup

        // Configure TokenValidationParameters once and register as singleton
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Read directly from configuration - ensure section exists
            ValidIssuer = configuration["AuthConfiguration:Issuer"],
            ValidAudience = configuration["AuthConfiguration:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration["AuthConfiguration:Key"] ?? throw new InvalidOperationException("JWT Key (AuthConfiguration:Key) is not configured in appsettings.json")))
        };
        services.AddSingleton(tokenValidationParameters);

        // Add JWT Bearer authentication scheme
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                // Use the singleton TokenValidationParameters
                options.TokenValidationParameters = tokenValidationParameters;
            });

        return services;
    }

    private static IServiceCollection AddClaimsAuthorization(this IServiceCollection services)
    {
        // Register our custom AuthorizationConfigureOptions which uses IPolicyFactory
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, AuthorizationConfigureOptions>();

        // Add the core Authorization services (needed for AddPolicy etc.)
        services.AddAuthorization();

        return services;
    }
}