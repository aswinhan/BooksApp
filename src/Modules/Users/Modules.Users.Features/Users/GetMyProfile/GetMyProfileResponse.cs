namespace Modules.Users.Features.Users.GetMyProfile;
// DTO to return user profile data
public record GetMyProfileResponse(
    string UserId,
    string Email,
    string? DisplayName,
    string? Street,
    string? City,
    string? State,
    string? ZipCode
);