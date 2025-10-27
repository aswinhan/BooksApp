using Microsoft.Extensions.Configuration; // To read keys
using Microsoft.Extensions.Logging;
using Modules.Common.Application.Payments; // Use interface
using Modules.Common.Domain.Results;
using Stripe; // Use Stripe SDK
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly PaymentIntentService _paymentIntentService;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _defaultCurrency = "inr"; // Or read from config

    // Inject IConfiguration to get keys
    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        // Set the API key for the Stripe SDK
        // Reads the SecretKey from User Secrets (or other providers)
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe SecretKey not configured.");

        _paymentIntentService = new PaymentIntentService();
        _logger = logger;
        // You might read default currency from config too
        // _defaultCurrency = configuration["Stripe:DefaultCurrency"] ?? "usd";
    }

    public async Task<Result<PaymentIntentResult>> CreateOrUpdatePaymentIntentAsync(
        string? paymentIntentId, long amount, string currency,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return Error.Validation("Payment.InvalidAmount", "Payment amount must be positive.");
        }

        currency = string.IsNullOrWhiteSpace(currency) ? _defaultCurrency : currency.ToLowerInvariant();

        try
        {
            PaymentIntent paymentIntent;
            if (string.IsNullOrEmpty(paymentIntentId))
            {
                // Create new Payment Intent
                var createOptions = new PaymentIntentCreateOptions
                {
                    Amount = amount, // Amount in smallest unit (e.g., paise for INR)
                    Currency = currency,
                    // Enable desired payment method types
                    PaymentMethodTypes = ["card"], // Example: Enable card payments
                    Metadata = metadata ?? new Dictionary<string, string>()
                    // Add customer ID if known: Customer = "cus_..."
                };
                _logger.LogInformation("Creating new Stripe Payment Intent for amount {Amount} {Currency}", amount, currency);
                paymentIntent = await _paymentIntentService.CreateAsync(createOptions, null, cancellationToken);
                _logger.LogInformation("Created Stripe Payment Intent: {IntentId}", paymentIntent.Id);
            }
            else
            {
                // Update existing Payment Intent (e.g., if cart total changed)
                var updateOptions = new PaymentIntentUpdateOptions
                {
                    Amount = amount,
                    Currency = currency, // Currency usually cannot be updated, check Stripe docs
                    Metadata = metadata ?? new Dictionary<string, string>()
                };
                _logger.LogInformation("Updating Stripe Payment Intent {IntentId} for amount {Amount} {Currency}", paymentIntentId, amount, currency);
                paymentIntent = await _paymentIntentService.UpdateAsync(paymentIntentId, updateOptions, null, cancellationToken);
                _logger.LogInformation("Updated Stripe Payment Intent: {IntentId}", paymentIntent.Id);
            }

            // Return essential details needed by the frontend
            return new PaymentIntentResult(
                paymentIntent.Id,
                paymentIntent.ClientSecret,
                paymentIntent.Status
            );
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe API error creating/updating Payment Intent. Status: {Status}, Code: {Code}", ex.HttpStatusCode, ex.StripeError?.Code);
            return Error.Failure("Payment.StripeError", $"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating/updating Payment Intent.");
            return Error.Unexpected("Payment.UnexpectedError", "An unexpected error occurred during payment processing.");
        }
    }
}