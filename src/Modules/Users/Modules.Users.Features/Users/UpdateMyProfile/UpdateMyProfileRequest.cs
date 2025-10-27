namespace Modules.Users.Features.Users.UpdateMyProfile;

// DTO for updating profile info
// All fields are optional except maybe DisplayName?
public record UpdateMyProfileRequest(
    string DisplayName, // Make display name required for update?
    string? Street,
    string? City,
    string? State,
    string? ZipCode
);