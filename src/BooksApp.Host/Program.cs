using BooksApp.Host.Seeding; // <-- Add using for seeding services
using Microsoft.EntityFrameworkCore;
using Modules.Blog.Features;
using Modules.Catalog.Features;
using Modules.Common.API;
using Modules.Common.API.Extensions;
using Modules.Common.Infrastructure;
using Modules.Common.Infrastructure.Database;
using Modules.Discounts.Features;
using Modules.Inventory.Features;
using Modules.Orders.Features;
using Modules.Users.Features;
using Serilog;
using StackExchange.Redis;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddCoreHostLogging();

    var services = builder.Services;
    var configuration = builder.Configuration;

    services.AddCoreWebApiInfrastructure();
    services.AddCoreInfrastructure(configuration, ["Users", "Catalog", "Orders", "Blog", "Inventory", "Discounts"]);
    services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection")
        ?? throw new InvalidOperationException("Redis connection string 'RedisConnection' not found.")));

    services.AddUsersModule(configuration);
    services.AddCatalogModule(configuration);
    services.AddOrdersModule(configuration);
    services.AddBlogModule(configuration);
    services.AddInventoryModule(configuration);
    services.AddDiscountsModule(configuration);

    services.AddScoped<CatalogSeedService>();
    services.AddScoped<BlogSeedService>();
    services.AddScoped<UserSeedService>();

    var app = builder.Build();

    // --- Configure Middleware Pipeline ---
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    // app.UseCors("_myAllowSpecificOrigins"); // Uncomment if needed

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseModuleMiddlewares();

    app.MapApiEndpoints();

    // --- Run Migrations & Seeding (ONLY ONCE and only in Development) ---
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            Log.Information("Applying database migrations...");
            await scope.MigrateModuleDatabasesAsync();
            Log.Information("Database migrations applied successfully.");

            Log.Information("Seeding initial data (Development)...");

            var userSeedService = scope.ServiceProvider.GetRequiredService<BooksApp.Host.Seeding.UserSeedService>();
            await userSeedService.SeedUsersAndRolesAsync();

            var catalogSeedService = scope.ServiceProvider.GetRequiredService<CatalogSeedService>();
            await catalogSeedService.SeedCatalogAsync();

            var blogSeedService = scope.ServiceProvider.GetRequiredService<BlogSeedService>();
            await blogSeedService.SeedBlogAsync();

            Log.Information("Initial data seeding complete.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during database migration or seeding.");
        }
    }

    // --- Run the App ---
    Log.Information("Starting BooksApp.Host...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BooksApp.Host terminated unexpectedly.");
}
finally
{
    Log.Information("Shutting down BooksApp.Host...");
    Log.CloseAndFlush();
}