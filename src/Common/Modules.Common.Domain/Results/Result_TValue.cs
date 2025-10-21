namespace Modules.Common.Domain.Results;

/// <summary>
/// Represents the outcome of an operation, holding either a success value (TValue) or errors.
/// Using 'partial' allows splitting the definition across files.
/// </summary>
public readonly partial record struct Result<TValue> : IResult<TValue>
{
    private readonly TValue? _value = default; // Holds the success value
    private readonly List<Error>? _errors = default; // Holds the list of errors

    // Constructor for a successful result
    private Result(TValue value)
    {
        // Null check for reference types; allows default for value types
        if (value is null && typeof(TValue).IsClass) // Check if TValue is a class before null check
        {
            throw new ArgumentNullException(nameof(value), "Success value cannot be null for reference types.");
        }
        _value = value;
        _errors = null; // Ensure errors is null for success
    }

    // Constructor for a failure result with a single error
    private Result(Error error)
    {
        _errors = [error]; // Use collection expression for single error
        _value = default; // Ensure value is default for error
    }

    // Constructor for a failure result with multiple errors
    private Result(List<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("Cannot create an error Result<TValue> with an empty error list.", nameof(errors));
        }
        _errors = errors;
        _value = default; // Ensure value is default for error
    }

    /// <summary>
    /// Gets a value indicating whether the state is a success.
    /// </summary>
    public bool IsSuccess => _errors is null; // Success if errors is null

    /// <summary>
    /// Gets a value indicating whether the state is error.
    /// </summary>
    public bool IsError => !IsSuccess; // Error if not success

    /// <summary>
    /// Gets the collection of errors. Returns an empty list if successful.
    /// </summary>
    public List<Error>? Errors => IsSuccess ? null : _errors;

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when IsError is true.</exception>
    public TValue? Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access Value when result IsError.");

    /// <summary>
    /// Gets the first error if IsError is true.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when IsSuccess is true.</exception>
    public Error FirstError => IsError ? _errors![0] : throw new InvalidOperationException("Cannot access FirstError when result IsSuccess.");

}