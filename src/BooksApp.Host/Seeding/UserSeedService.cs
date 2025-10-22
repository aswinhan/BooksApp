using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // For AnyAsync
using Microsoft.Extensions.Logging;
using Modules.Users.Domain.Users; // Need User and Role
using System;
using System.Linq;
using System.Security.Claims; // For Claims
using System.Threading.Tasks;

namespace BooksApp.Host.Seeding;

public class UserSeedService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager, // Inject RoleManager
    ILogger<UserSeedService> logger)
{
    // Define role names
    private const string AdminRole = "Admin";
    private const string UserRole = "User"; // Standard user role

    public async Task SeedUsersAndRolesAsync()
    {
        await SeedRolesAsync(); // Ensure roles exist first
        await SeedAdminUserAsync(); // Then seed users
        // Add seeding for regular users if needed
    }

    private async Task SeedRolesAsync()
    {
        logger.LogInformation("Checking/Seeding roles...");

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            logger.LogInformation("Creating {RoleName} role.", AdminRole);
            var adminRole = new Role { Id = Guid.NewGuid().ToString(), Name = AdminRole, NormalizedName = AdminRole.ToUpperInvariant() };
            var result = await roleManager.CreateAsync(adminRole);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create {RoleName} role: {@Errors}", AdminRole, result.Errors);
                // Decide how to handle failure - throw?
                return; // Stop if role creation fails
            }
            // *** Add Admin Claims Here ***
            // Grant all permissions to Admin role
            await AddPermissionClaim(adminRole, "users:read");
            await AddPermissionClaim(adminRole, "users:create");
            await AddPermissionClaim(adminRole, "users:update");
            await AddPermissionClaim(adminRole, "users:delete");
            // Add claims for Catalog, Blog, Orders policies...
            logger.LogInformation("Added permissions to {RoleName} role.", AdminRole);
        }
        else
        {
            logger.LogInformation("{RoleName} role already exists.", AdminRole);
        }


        if (!await roleManager.RoleExistsAsync(UserRole))
        {
            logger.LogInformation("Creating {RoleName} role.", UserRole);
            var userRole = new Role { Id = Guid.NewGuid().ToString(), Name = UserRole, NormalizedName = UserRole.ToUpperInvariant() };
            var result = await roleManager.CreateAsync(userRole);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create {RoleName} role: {@Errors}", UserRole, result.Errors);
                return;
            }
            // Add standard user claims if needed (e.g., read own orders)
        }
        else
        {
            logger.LogInformation("{RoleName} role already exists.", UserRole);
        }
        logger.LogInformation("Role seeding finished.");
    }

    // Helper to add claims to a role
    private async Task AddPermissionClaim(Role role, string permission)
    {
        var allClaims = await roleManager.GetClaimsAsync(role);
        if (!allClaims.Any(c => c.Type == permission && c.Value == "true"))
        {
            var result = await roleManager.AddClaimAsync(role, new Claim(permission, "true"));
            if (!result.Succeeded)
            {
                logger.LogError("Failed to add claim {Permission} to role {RoleName}: {@Errors}", permission, role.Name, result.Errors);
            }
        }
    }


    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@booksapp.com"; // Use a specific admin email
        logger.LogInformation("Checking/Seeding admin user {AdminEmail}...", adminEmail);

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            logger.LogInformation("Creating admin user {AdminEmail}.", adminEmail);
            var adminUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true, // Confirm email for seed user
                CreatedAtUtc = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Use a strong default password
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create admin user {AdminEmail}: {@Errors}", adminEmail, result.Errors);
                return; // Stop if user creation fails
            }

            // Assign the Admin role
            var roleResult = await userManager.AddToRoleAsync(adminUser, AdminRole);
            if (!roleResult.Succeeded)
            {
                logger.LogError("Failed to assign {RoleName} role to admin user {AdminEmail}: {@Errors}", AdminRole, adminEmail, roleResult.Errors);
            }
            else
            {
                logger.LogInformation("Assigned {RoleName} role to admin user {AdminEmail}.", AdminRole, adminEmail);
            }
        }
        else
        {
            logger.LogInformation("Admin user {AdminEmail} already exists.", adminEmail);
        }
        logger.LogInformation("Admin user seeding finished.");
    }
}