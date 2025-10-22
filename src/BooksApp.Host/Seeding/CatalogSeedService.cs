using Bogus; // For generating fake data
using Microsoft.EntityFrameworkCore; // For AnyAsync, AddRangeAsync
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities; // Need Author, Book
using Modules.Catalog.Infrastructure.Database; // Need CatalogDbContext
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksApp.Host.Seeding; // Corrected namespace

public class CatalogSeedService(
    CatalogDbContext catalogDbContext,
    ILogger<CatalogSeedService> logger)
{
    private const int NumberOfAuthors = 5;
    private const int BooksPerAuthorMin = 3;
    private const int BooksPerAuthorMax = 10;

    public async Task SeedCatalogAsync()
    {
        // Set Bogus seed for reproducibility
        Randomizer.Seed = new Random(12345);

        if (await catalogDbContext.Authors.AnyAsync() || await catalogDbContext.Books.AnyAsync())
        {
            logger.LogInformation("Catalog data already exists, skipping seeding.");
            return;
        }

        logger.LogInformation("Starting Catalog data seeding...");

        // --- Seed Authors ---
        var authorFaker = new Faker<Author>()
            .CustomInstantiator(f => new Author(
                Guid.NewGuid(),
                f.Name.FullName(), // Generate fake author name
                f.Lorem.Paragraph(2) // Generate fake biography
             ));

        var authors = authorFaker.Generate(NumberOfAuthors);
        await catalogDbContext.Authors.AddRangeAsync(authors);
        await catalogDbContext.SaveChangesAsync(); // Save authors to get IDs
        logger.LogInformation("Seeded {Count} authors.", authors.Count);

        // --- Seed Books ---
        var bookFaker = new Faker<Book>()
            .CustomInstantiator(f =>
            {
                var author = f.PickRandom(authors); // Pick a random seeded author
                return new Book(
                    Guid.NewGuid(),
                    f.Commerce.ProductName(), // Generate plausible book title
                    f.Lorem.Paragraphs(2), // Generate description
                    f.Random.ReplaceNumbers("############"), // Generate 13-digit ISBN
                    f.Random.Decimal(5.00m, 50.00m), // Price between 5.00 and 50.00
                    author.Id // Assign author ID
                );
            });

        var allBooks = new List<Book>();
        foreach (var author in authors)
        {
            int bookCount = Randomizer.Seed.Next(BooksPerAuthorMin, BooksPerAuthorMax + 1);
            var booksForAuthor = bookFaker.Generate(bookCount);
            // Manually set AuthorId again just to be safe (though constructor does it)
            booksForAuthor.ForEach(b => { /* Constructor sets AuthorId */ });
            allBooks.AddRange(booksForAuthor);
        }

        await catalogDbContext.Books.AddRangeAsync(allBooks);
        await catalogDbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} books.", allBooks.Count);

        logger.LogInformation("Catalog data seeding completed.");
    }
}