using Modules.Common.Domain.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Common.Application.Payments;

// DTO for payment intent result
public record PaymentIntentResult(string IntentId, string ClientSecret, string Status);

public interface IPaymentService
{
    /// <summary>
    /// Creates a new payment intent or updates an existing one for a given amount.
    /// </summary>
    /// <param name="paymentIntentId">Existing ID to update, or null to create new.</param>
    /// <param name="amount">Amount in the smallest currency unit (e.g., cents).</param>
    /// <param name="currency">Currency code (e.g., "usd", "inr").</param>
    /// <param name="metadata">Optional metadata (e.g., OrderId).</param>
    /// <returns>Result containing payment intent details or an error.</returns>
    Task<Result<PaymentIntentResult>> CreateOrUpdatePaymentIntentAsync(
        string? paymentIntentId,
        long amount,
        string currency,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    // Add methods for handling webhooks, refunds etc. later
}