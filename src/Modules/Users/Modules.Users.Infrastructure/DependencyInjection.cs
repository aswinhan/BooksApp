using Microsoft.AspNetCore.Identity; // For AddIdentityCore, AddRoles etc.
using Microsoft.EntityFrameworkCore; // For UseNpgsql
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database; // For IModuleDatabaseMigrator, AuditableInterceptor
using Modules.Common.Infrastructure.Policies; // For IPolicyFactory
using Modules.Users.Domain.Authentication; // For IClientAuthorizationService
using Modules.Users.Domain.Users; // For User, Role
using Modules.Users.Infrastructure.Authorization; // For ClientAuthorizationService
using Modules.Users.Infrastructure.Database; // For UsersDbContext, DbConsts etc.
using Modules.Users.Infrastructure.Policies; // For UsersPolicyFactory

// Put extensions into the global Microsoft namespace for easy discovery
// ReSharper disable once CheckNamespace
namespace Modules.Users.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers services specific to the Users module's infrastructure layer.
    /// </summary>
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);

        // Register the implementation for the domain service interface
        services.AddScoped<IClientAuthorizationService, ClientAuthorizationService>();

        // Register this module's policy factory
        services.AddSingleton<IPolicyFactory, UsersPolicyFactory>();

        return services;
    }

    // Private helper to configure DbContext and Identity
    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres"); // Use the main connection string

        // Add the DbContext specific to this module
        services.AddDbContext<UsersDbContext>((serviceProvider, options) =>
        {
            // Resolve the shared AuditableInterceptor
            var interceptor = serviceProvider.GetRequiredService<AuditableInterceptor>();

            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                {
                    // Configure migrations history table within the module's schema
                    npgsqlOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema);
                })
                .AddInterceptors(interceptor) // Add the interceptor
                .UseSnakeCaseNamingConvention(); // Apply naming convention
        });

        // Register the migrator for this module's DbContext
        services.AddScoped<IModuleDatabaseMigrator, UsersDatabaseMigrator>();

        // Add ASP.NET Core Identity, configuring it to use our custom entities and DbContext
        services
            .AddIdentityCore<User>(options =>
            {
                // Configure Identity options (e.g., password requirements)
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false; // Match template
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<Role>() // Add role management
            .AddEntityFrameworkStores<UsersDbContext>() // Use our DbContext
            .AddSignInManager() // Add SignInManager for password checking
            .AddDefaultTokenProviders(); // For things like password reset tokens
    }
}   