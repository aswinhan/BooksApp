using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Notifications.Features.Shared.Routes;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Notifications.Features.Contact;

public class SubmitContactFormEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(NotificationRouteConsts.SubmitContactForm, Handle).AllowAnonymous()
           .WithName("SubmitContactForm").Produces(StatusCodes.Status204NoContent) // Success
           .ProducesValidationProblem().ProducesProblem(StatusCodes.Status500InternalServerError).WithTags("Notifications");
    }
    private static async Task<IResult> Handle([FromBody] SubmitContactFormRequest req, IValidator<SubmitContactFormRequest> v, ISubmitContactFormHandler h, CancellationToken ct)
    {
        var valResult = await v.ValidateAsync(req, ct); if (!valResult.IsValid) return Results.ValidationProblem(valResult.ToDictionary());
        var resp = await h.HandleAsync(req, ct); return resp.IsError ? resp.Errors.ToProblem() : Results.NoContent();
    }
}