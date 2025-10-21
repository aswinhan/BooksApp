using Microsoft.AspNetCore.Http; // Required for IResult, Results, StatusCodes
using Modules.Common.Domain.Results; // Required for Error, ErrorType
using System.Collections.Generic; // Required for List, Dictionary
using System.Linq; // Required for First(), ToDictionary()

// Put into Microsoft.AspNetCore.Http namespace for easy discovery on Results
namespace Modules.Common.API.Extensions;


public static class EndpointResultsExtensions
{
    /// <summary>
    /// Converts a list of domain Errors into an appropriate IResult (ProblemDetails).
    /// </summary>
    public static Microsoft.AspNetCore.Http.IResult ToProblem(this List<Error>? errors)
    {
        if (errors is null || errors.Count == 0)
        {
            // Fallback for unexpected empty error list
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
        }

        // Determine status code based on the *first* error's type
        var statusCode = errors.First().Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status400BadRequest, // Map general failure to Bad Request
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError, // Map unexpected to Server Error
            ErrorType.Custom => StatusCodes.Status400BadRequest, // Map custom to Bad Request by default
            _ => StatusCodes.Status500InternalServerError // Default fallback
        };

        // Use ValidationProblem to return multiple errors cleanly
        // Convert List<Error> to Dictionary<string, string[]>
        var errorDictionary = errors
             .GroupBy(e => e.Code) // Group by error code to avoid duplicate keys
             .ToDictionary(
                 group => group.Key,
                 group => group.Select(e => e.Description).ToArray()
             );


        return Results.ValidationProblem(
             errors: errorDictionary,
             statusCode: statusCode,
             title: GetTitleFromStatusCode(statusCode) // Add a standard title
         );
    }

    private static string GetTitleFromStatusCode(int statusCode) =>
         statusCode switch
         {
             StatusCodes.Status400BadRequest => "Bad Request",
             StatusCodes.Status401Unauthorized => "Unauthorized",
             StatusCodes.Status403Forbidden => "Forbidden",
             StatusCodes.Status404NotFound => "Not Found",
             StatusCodes.Status409Conflict => "Conflict",
             _ => "An error occurred" // Default title
         };
}