using FluentValidation.Results;
using Modules.Common.Domain.Results;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Inventory.Features.Features.Shared.Errors;

internal static class ValidationExtensions
{
    // Helper to convert FluentValidation errors to our Domain Error type
    internal static List<Error> ToDomainErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .Select(x => Error.Validation(
                $"Inventory.{x.PropertyName}", // Use property name in code
                x.ErrorMessage))
            .ToList();
    }
}