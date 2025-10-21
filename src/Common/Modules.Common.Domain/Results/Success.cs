namespace Modules.Common.Domain.Results;

/// <summary>
/// Represents a successful result without a specific value.
/// Just indicates completion.
/// </summary>
public readonly record struct Success;

/// <summary>
/// Static class to provide easy access to a generic Success result.
/// </summary>
public static class Result
{
    /// <summary>
    /// Represents a successful operation result. Usage: return Result.Success;
    /// </summary>
    public static Success Success => default; // Uses the default value of the Success struct
}