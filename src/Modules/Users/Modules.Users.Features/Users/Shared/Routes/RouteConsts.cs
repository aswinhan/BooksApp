namespace Modules.Users.Features.Users.Shared.Routes;

// Defines the API route constants for this module
internal static class RouteConsts
{
    private const string BaseRoute = "/api/users"; // Base path for all user endpoints

    // Specific endpoint paths
    internal const string GetMyProfile = $"{BaseRoute}/me";
    internal const string GetById = $"{BaseRoute}/{{userId}}"; // e.g., /api/users/guid-guid-guid
    internal const string Login = $"{BaseRoute}/login";
    internal const string Register = $"{BaseRoute}/register";
    internal const string RefreshToken = $"{BaseRoute}/refresh";
    internal const string UpdateUser = $"{BaseRoute}/{{userId}}";
    internal const string DeleteUser = $"{BaseRoute}/{{userId}}";
    internal const string UpdateUserRole = $"{BaseRoute}/{{userId}}/role";
}