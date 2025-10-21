using Microsoft.AspNetCore.Builder; // Required for WebApplicationBuilder
using Microsoft.AspNetCore.Http.Json; // Required for JsonOptions
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models; // Required for OpenApiInfo, etc.
using Modules.Common.API.ErrorHandling; // Required for GlobalExceptionHandler
using Serilog; // Required for UseSerilog
using System.Diagnostics.CodeAnalysis; // Required for SuppressMessage
using System.Text.Json.Serialization; // Required for JsonStringEnumConverter

// Put extensions into the global Microsoft namespace for easy discovery
// ReSharper disable once CheckNamespace
namespace Modules.Common.API;

public static class DependencyInjection
{
    /// <summary>
    /// Registers core Web API infrastructure services (Swagger, Error Handling, ProblemDetails).
    /// </summary>
    public static IServiceCollection AddCoreWebApiInfrastructure(this IServiceCollection services)
    {
        services
            .AddEndpointsApiExplorer() // Needed for Swagger
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "BooksApp API", Version = "v1" });

                // Configure Swagger to use JWT Bearer authentication
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT token (without 'Bearer ' prefix)",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http, // Use Http type
                    BearerFormat = "JWT",
                    Scheme = "bearer" // Scheme must be lowercase 'bearer'
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer" // Must match the definition ID
                            }
                        },
                        Array.Empty<string>() // No specific scopes required for now
                    }
                });
            });

        // Register the custom global exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        // Add required services for ProblemDetails generation
        services.AddProblemDetails();

        // Configure JSON options (e.g., serialize enums as strings)
        services.Configure<JsonOptions>(opt =>
        {
            opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }

    /// <summary>
    /// Configures Serilog as the primary logging provider for the host.
    /// </summary>
    public static void AddCoreHostLogging(this WebApplicationBuilder builder)
    {
        // Clear default providers and use Serilog, reading config from appsettings
        builder.Logging.ClearProviders(); // Clear default logging providers
        builder.Host.UseSerilog((context, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(context.Configuration));
    }
}