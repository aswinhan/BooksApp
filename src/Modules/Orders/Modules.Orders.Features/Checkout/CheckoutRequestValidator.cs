using FluentValidation;
using Modules.Orders.Domain.Enums;
using Modules.Orders.Domain.ValueObjects; // Required for Address

namespace Modules.Orders.Features.Checkout;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.")
            .SetValidator(new AddressValidator()); // Reuse Address validator if needed

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method specified.")
            .NotEqual(PaymentMethod.Undefined).WithMessage("Payment method must be selected.");

        // BillingAddress is required ONLY if UseShippingAddressForBilling is false
        RuleFor(x => x.BillingAddress)
            .NotNull().WithMessage("Billing address is required when not using shipping address.")
            .SetValidator(new AddressValidator()!) // Use ! if validator handles null internally
            .When(x => !x.UseShippingAddressForBilling); // Only apply when flag is false
    }
}

// Define or reuse Address validator if not already globally available
// Example: (Put this in a shared location if reused)
public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(address => address.Street).NotEmpty().MaximumLength(200);
        RuleFor(address => address.City).NotEmpty().MaximumLength(100);
        RuleFor(address => address.State).NotEmpty().MaximumLength(100);
        RuleFor(address => address.ZipCode).NotEmpty().MaximumLength(20);
    }
}