using Modules.Common.Domain.Results;
using System;

namespace Modules.Wishlist.Domain.Errors;

public static class WishlistErrors
{
    private const string Prefix = "Wishlist";
    public static Error ItemAlreadyExists(Guid bookId) => Error.Conflict($"{Prefix}.AlreadyExists", $"Book {bookId} is already in the wishlist.");
    public static Error ItemNotFound(Guid bookId) => Error.NotFound($"{Prefix}.NotFound", $"Book {bookId} not found in the wishlist.");
    public static Error BookNotFound(Guid bookId) => Error.NotFound($"{Prefix}.BookNotFound", $"Book {bookId} does not exist."); // If checking Catalog
}