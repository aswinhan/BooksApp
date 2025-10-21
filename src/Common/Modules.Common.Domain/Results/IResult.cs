namespace Modules.Common.Domain.Results;

/// <summary>
/// Base interface for all results.
/// </summary>
public interface IResult
{
    /// <summary>
    /// Gets the list of errors. Null or empty if successful.
    /// </summary>
    List<Error>? Errors { get; }

    /// <summary>
    /// Gets a value indicating whether the result is successful (no errors).
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is an error (has errors).
    /// </summary>
    bool IsError { get; }
}

/// <summary>
/// Interface for results that carry a value on success.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public interface IResult<out TValue> : IResult
{
    /// <summary>
    /// Gets the success value. Throws an exception if accessed when IsError is true.
    /// </summary>
    TValue? Value { get; }
}