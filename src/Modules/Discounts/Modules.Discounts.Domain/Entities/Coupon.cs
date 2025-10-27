using Modules.Common.Domain;
using Modules.Common.Domain.Results;
using Modules.Discounts.Domain.Enums;
using System;

namespace Modules.Discounts.Domain.Entities;

public class Coupon : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!; // The code users enter (e.g., "SUMMER10")
    public DiscountType Type { get; private set; }
    public decimal Value { get; private set; } // Percentage (e.g., 10.00) or Fixed Amount (e.g., 5.00)
    public DateTime? ExpiryDate { get; private set; } // Optional expiration
    public int UsageLimit { get; private set; } // Max times the coupon can be used overall
    public int UsageCount { get; private set; } // How many times it has been used
    public decimal MinimumCartAmount { get; private set; } // Minimum cart total required
    public bool IsActive { get; private set; }

    private Coupon() { } // EF Core

    public Coupon(Guid id, string code, DiscountType type, decimal value, DateTime? expiryDate, int usageLimit, decimal minimumCartAmount)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Discount value must be positive.");
        if (usageLimit < 0) throw new ArgumentOutOfRangeException(nameof(usageLimit), "Usage limit cannot be negative.");
        if (minimumCartAmount < 0) throw new ArgumentOutOfRangeException(nameof(minimumCartAmount), "Minimum amount cannot be negative.");
        if (type == DiscountType.Percentage && value > 100) throw new ArgumentOutOfRangeException(nameof(value), "Percentage discount cannot exceed 100.");


        Id = id;
        Code = code.ToUpperInvariant(); // Store codes consistently
        Type = type;
        Value = value;
        ExpiryDate = expiryDate;
        UsageLimit = usageLimit;
        MinimumCartAmount = minimumCartAmount;
        UsageCount = 0; // Starts at 0
        IsActive = true; // Active by default
        CreatedAtUtc = DateTime.UtcNow;
    }

    // --- Domain Logic ---
    public Result<Success> Validate(decimal cartTotal)
    {
        if (!IsActive) return Error.Validation("Discount.Inactive", "Coupon is not active.");
        if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow) return Error.Validation("Discount.Expired", "Coupon has expired.");
        if (UsageCount >= UsageLimit) return Error.Validation("Discount.LimitReached", "Coupon usage limit reached.");
        if (cartTotal < MinimumCartAmount) return Error.Validation("Discount.MinAmountNotMet", $"Minimum cart amount of {MinimumCartAmount:C} not met.");

        return Result.Success;
    }

    public decimal CalculateDiscount(decimal cartTotal)
    {
        if (!Validate(cartTotal).IsSuccess) return 0m; // Return 0 if invalid

        decimal discount = Type switch
        {
            DiscountType.Percentage => cartTotal * (Value / 100m),
            DiscountType.FixedAmount => Value,
            _ => 0m
        };

        // Ensure discount doesn't exceed cart total
        return Math.Min(discount, cartTotal);
    }

    public Result<Success> RecordUsage()
    {
        // Re-validate before recording usage
        // Note: In a high-concurrency scenario, this check + increment needs atomicity (handled by SaveChanges concurrency control)
        if (!IsActive) return Error.Validation("Discount.Inactive", "Coupon is not active.");
        if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow) return Error.Validation("Discount.Expired", "Coupon has expired.");
        if (UsageCount >= UsageLimit) return Error.Validation("Discount.LimitReached", "Coupon usage limit reached.");


        UsageCount++;
        UpdatedAtUtc = DateTime.UtcNow;
        // Optionally deactivate if UsageCount reaches UsageLimit
        // if (UsageCount >= UsageLimit) IsActive = false;

        return Result.Success;
    }

    public void Update(DiscountType type, decimal value, DateTime? expiryDate, int usageLimit, decimal minimumCartAmount, bool isActive)
    {
        // Add validation similar to constructor
        Type = type;
        Value = value;
        ExpiryDate = expiryDate;
        UsageLimit = usageLimit;
        MinimumCartAmount = minimumCartAmount;
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }


    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}