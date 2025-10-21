namespace Modules.Common.Domain.Results;

/// <summary>
/// Contains implicit operators for easy creation of Result<TValue> instances.
/// </summary>
public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Implicitly converts a value TValue to a successful Result<TValue>.
    /// Allows: Result<int> result = 5;
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => new(value);

    /// <summary>
    /// Implicitly converts an Error to a failed Result<TValue>.
    /// Allows: Result<int> result = Error.NotFound(...);
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => new(error);

    /// <summary>
    /// Implicitly converts a List of Errors to a failed Result<TValue>.
    /// Allows: Result<int> result = new List<Error>{ Error.Validation(...) };
    /// </summary>
    public static implicit operator Result<TValue>(List<Error> errors) => new(errors);

    /// <summary>
    /// Implicitly converts an array of Errors to a failed Result<TValue>.
    /// Allows: Result<int> result = new Error[]{ Error.Validation(...) };
    /// </summary>
    public static implicit operator Result<TValue>(Error[] errors) => new([.. errors]); // Use collection expression
}