using FluentValidation.Results;
using Modules.Common.Domain.Results;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Discounts.Features.Features.Shared.Errors;

internal static class ValidationExtensions
{
    internal static List<Error> ToDomainErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .Select(x => Error.Validation($"Discount.{x.PropertyName}", x.ErrorMessage))
            .ToList();
    }
}