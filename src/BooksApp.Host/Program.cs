using Microsoft.EntityFrameworkCore; // Needed for MigrateAsync
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Features;
using Modules.Common.API;
using Modules.Common.API.Extensions; // For UseModuleMiddlewares
using Modules.Common.Infrastructure;
using Modules.Common.Infrastructure.Database; // For MigrateModuleDatabasesAsync
using Modules.Orders.Features;
using Modules.Users.Features;
using Serilog; // Required for static Log class if using bootstrap logger pattern
using StackExchange.Redis;

// --- Bootstrap Logger (Optional but Recommended) ---
// Configure Serilog for early logging before the host builds
// Log.Logger = new LoggerConfiguration()
//    .WriteTo.Console()
//    .CreateBootstrapLogger();
// Log.Information("Starting BooksApp.Host...");

try // Wrap startup in try/catch for bootstrap logging
{
    var builder = WebApplication.CreateBuilder(args);

    // --- Logging ---
    // Use Serilog configured via Common.API extension
    builder.AddCoreHostLogging();

    // --- Add Services ---
    var services = builder.Services;
    var configuration = builder.Configuration;

    // 1. Add Core Web API Infrastructure (Swagger, Error Handling, ProblemDetails)
    services.AddCoreWebApiInfrastructure();

    // 2. Add Core Infrastructure (Auth, Policies, Caching, Telemetry, AuditableInterceptor)
    services.AddCoreInfrastructure(configuration,
        // List module activity names for OpenTelemetry tracing
        [
            // Add other module names here later (e.g., CatalogModuleRegistration.ActivityModuleName)
             "Users", // Placeholder name - we'll define a constant later
             "Catalog",
             "Orders"
        ]);

    // --- Add Redis Connection ---
    // Register Redis connection multiplexer as a singleton
    services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection")
            ?? throw new InvalidOperationException("Redis connection string 'RedisConnection' not found.")));

    // 3. Add Module Services (This registers everything for the Users module)
    services.AddUsersModule(configuration);
    services.AddCatalogModule(configuration);
    services.AddOrdersModule(configuration);

    // --- Build the App ---
    var app = builder.Build();

    // --- Configure Middleware Pipeline (Order Matters!) ---

    // Enable Serilog request logging
    app.UseSerilogRequestLogging();

    // Global Exception Handler (must be early)
    app.UseExceptionHandler(); // Let the registered handler handle it

    // Configure Swagger in Development
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // CORS - Add if needed for your frontend (using policy from Common.Infrastructure)
    // app.UseCors("_myAllowSpecificOrigins"); // Make sure policy is defined if used

    // Authentication & Authorization (Core ASP.NET Identity middleware)
    app.UseAuthentication();
    app.UseAuthorization();

    // Custom Module Middleware (e.g., Users token revocation check)
    app.UseModuleMiddlewares();

    // Map Endpoints discovered from all modules
    app.MapApiEndpoints();

    // --- Database Migrations (Run on startup in Development) ---
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            Log.Information("Applying database migrations...");
            await scope.MigrateModuleDatabasesAsync(); // Runs migrators for all registered modules
            Log.Information("Database migrations applied successfully.");

            // Add User/Role Seeding here later if needed
            // var userSeedService = scope.ServiceProvider.GetRequiredService<UserSeedService>();
            // await userSeedService.SeedUsersAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during database migration or seeding.");
            // Decide if you want the app to stop if migrations fail
            // throw;
        }
    }


    // --- Run the App ---
    Log.Information("Starting BooksApp.Host...");
    await app.RunAsync();

}
catch (Exception ex) // Catch bootstrap errors
{
    Log.Fatal(ex, "BooksApp.Host terminated unexpectedly.");
}
finally
{
    Log.Information("Shutting down BooksApp.Host...");
    Log.CloseAndFlush(); // Ensure all logs are written
}