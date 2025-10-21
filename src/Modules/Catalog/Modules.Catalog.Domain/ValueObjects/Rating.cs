namespace Modules.Catalog.Domain.ValueObjects;

public class Rating
{
    public int Value { get; private set; }

    // Private constructor for EF Core and factory method
    private Rating() { }

    private Rating(int value)
    {
        Value = value;
    }

    // Public factory method to create a valid Rating
    public static Rating Create(int value)
    {
        if (value < 1 || value > 5)
        {
            // Throw an exception for invalid data during creation
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5.");
        }
        return new Rating(value);
    }
}