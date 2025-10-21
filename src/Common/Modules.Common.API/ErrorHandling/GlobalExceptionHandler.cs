using Microsoft.AspNetCore.Diagnostics; // Required for IExceptionHandler
using Microsoft.AspNetCore.Http; // Required for HttpContext, StatusCodes
using Microsoft.AspNetCore.Mvc; // Required for ProblemDetails
using Microsoft.Extensions.Logging; // Required for ILogger
using System; // Required for Exception
using System.Threading; // Required for CancellationToken
using System.Threading.Tasks; // Required for ValueTask

namespace Modules.Common.API.ErrorHandling;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService, // Service to write ProblemDetails
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Use the built-in ProblemDetailsService to write a standard error response
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails // Create a basic ProblemDetails object
            {
                Status = StatusCodes.Status500InternalServerError, // Redundant but clear
                Type = exception.GetType().Name, // Use exception type name
                Title = "An unexpected error occurred while processing your request.",
                Detail = exception.Message // Include exception message
            }
        });
    }
}