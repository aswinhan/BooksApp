namespace Modules.Common.Domain.Results;

public enum ErrorType
{
    Failure,     // General failure
    Unexpected,  // Unexpected error (like an exception)
    Validation,  // Input validation failed
    Conflict,    // Resource already exists or state conflict
    NotFound,    // Resource not found
    Unauthorized,// Authentication failed or missing
    Forbidden,   // User is authenticated but not allowed
    Custom       // For custom error types if needed
}