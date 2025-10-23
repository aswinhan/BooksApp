using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // For AnyAsync
using Microsoft.Extensions.Logging;
using Modules.Blog.Domain.Policies; // <-- ADD Blog Policies
using Modules.Catalog.Domain.Policies; // <-- ADD Catalog Policies
using Modules.Orders.Domain.Policies; // <-- ADD Orders Policies
using Modules.Users.Domain.Policies; // <-- ADD Users Policies (if separate from const string)
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
    // Add other roles if needed, e.g., private const string ManagerRole = "Manager";

    public async Task SeedUsersAndRolesAsync()
    {
        await SeedRolesAsync(); // Ensure roles exist first
        await SeedAdminUserAsync(); // Then seed users
        // Add seeding for regular users if needed (e.g., SeedRegularUserAsync)
    }

    private async Task SeedRolesAsync()
    {
        logger.LogInformation("Checking/Seeding roles...");

        // --- Seed Admin Role ---
        var adminRole = await roleManager.FindByNameAsync(AdminRole);
        if (adminRole == null)
        {
            logger.LogInformation("Creating {RoleName} role.", AdminRole);
            adminRole = new Role { Id = Guid.NewGuid().ToString(), Name = AdminRole, NormalizedName = AdminRole.ToUpperInvariant() };
            var result = await roleManager.CreateAsync(adminRole);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create {RoleName} role: {@Errors}", AdminRole, result.Errors);
                return; // Stop if role creation fails
            }
        }
        else
        {
            logger.LogInformation("{RoleName} role already exists.", AdminRole);
        }

        // --- Grant Claims to Admin Role ---
        logger.LogInformation("Granting claims to {RoleName} role...", AdminRole);
        await AddPermissionClaim(adminRole, UserPolicyConsts.ReadPolicy);
        await AddPermissionClaim(adminRole, UserPolicyConsts.CreatePolicy);
        await AddPermissionClaim(adminRole, UserPolicyConsts.UpdatePolicy);
        await AddPermissionClaim(adminRole, UserPolicyConsts.DeletePolicy);
        await AddPermissionClaim(adminRole, CatalogPolicyConsts.ManageCatalogPolicy);
        await AddPermissionClaim(adminRole, BlogPostPolicyConsts.ManageAllPostsPolicy);
        await AddPermissionClaim(adminRole, OrderPolicyConsts.ManageOrdersPolicy);
        await AddPermissionClaim(adminRole, OrderPolicyConsts.ViewAllOrdersPolicy);
        // Add claims for future modules...
        logger.LogInformation("Finished granting claims to {RoleName} role.", AdminRole);


        // --- Seed User Role ---
        var userRole = await roleManager.FindByNameAsync(UserRole);
        if (userRole == null)
        {
            logger.LogInformation("Creating {RoleName} role.", UserRole);
            userRole = new Role { Id = Guid.NewGuid().ToString(), Name = UserRole, NormalizedName = UserRole.ToUpperInvariant() };
            var result = await roleManager.CreateAsync(userRole);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create {RoleName} role: {@Errors}", UserRole, result.Errors);
                return;
            }
        }
        else
        {
            logger.LogInformation("{RoleName} role already exists.", UserRole);
        }

        // --- Grant Claims to User Role ---
        logger.LogInformation("Granting claims to {RoleName} role...", UserRole);
        await AddPermissionClaim(userRole, BlogPostPolicyConsts.AddCommentsPolicy); // Users can comment
        // Add other basic user permissions if needed (e.g., viewing own profile, etc.)
        logger.LogInformation("Finished granting claims to {RoleName} role.", UserRole);


        // --- Seed Other Roles (e.g., Manager) if needed ---
        // Repeat the pattern for other roles like "Manager"

        logger.LogInformation("Role seeding finished.");
    }

    // Helper to add claims idempotently
    private async Task AddPermissionClaim(Role role, string permission)
    {
        var allClaims = await roleManager.GetClaimsAsync(role);
        if (!allClaims.Any(c => c.Type == permission && c.Value == "true"))
        {
            logger.LogDebug("Adding claim {Permission} to role {RoleName}", permission, role.Name);
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

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            logger.LogInformation("Creating admin user {AdminEmail}.", adminEmail);
            adminUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true, // Confirm email for seed user
                CreatedAtUtc = DateTime.UtcNow
            };

            // Use a secure, configurable password (consider reading from config in real app)
            var result = await userManager.CreateAsync(adminUser, "AdminPass123!");
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create admin user {AdminEmail}: {@Errors}", adminEmail, result.Errors);
                return; // Stop if user creation fails
            }
            logger.LogInformation("Created admin user {AdminEmail}.", adminEmail);
        }
        else
        {
            logger.LogInformation("Admin user {AdminEmail} already exists.", adminEmail);
        }

        // Ensure Admin user has Admin role
        if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
        {
            logger.LogInformation("Assigning {RoleName} role to admin user {AdminEmail}.", AdminRole, adminEmail);
            var roleResult = await userManager.AddToRoleAsync(adminUser, AdminRole);
            if (!roleResult.Succeeded)
            {
                logger.LogError("Failed to assign {RoleName} role to admin user {AdminEmail}: {@Errors}", AdminRole, adminEmail, roleResult.Errors);
            }
        }
        else
        {
            logger.LogDebug("Admin user {AdminEmail} already in {RoleName} role.", adminEmail, AdminRole);
        }


        logger.LogInformation("Admin user seeding finished.");
    }
}