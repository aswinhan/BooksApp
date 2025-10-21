namespace Modules.Orders.Domain.ValueObjects;

// Represents a shipping address - defined by its components
public class Address
{
    public string Street { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!; // Or province, region, etc.
    public string ZipCode { get; private set; } = null!;
    // Consider adding Country if needed

    // Private constructor for EF Core
    private Address() { }

    // Public constructor for creation
    public Address(string street, string city, string state, string zipCode)
    {
        // Add validation if needed (e.g., non-empty strings)
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
    }
}