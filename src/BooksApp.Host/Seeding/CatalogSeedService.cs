using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities; // Need Author, Book, Category
using Modules.Catalog.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksApp.Host.Seeding;

public class CatalogSeedService(
    CatalogDbContext catalogDbContext,
    ILogger<CatalogSeedService> logger)
{
    private const int NumberOfAuthors = 5;
    private const int NumberOfCategories = 4; // Define how many categories to seed
    private const int BooksPerAuthorMin = 3;
    private const int BooksPerAuthorMax = 10;

    public async Task SeedCatalogAsync()
    {
        Randomizer.Seed = new Random(12345);

        // Check if ANY data exists to prevent re-seeding
        if (await catalogDbContext.Authors.AnyAsync() ||
            await catalogDbContext.Books.AnyAsync() ||
            await catalogDbContext.Categories.AnyAsync())
        {
            logger.LogInformation("Catalog data already exists, skipping seeding.");
            return;
        }

        logger.LogInformation("Starting Catalog data seeding...");

        // --- Seed Categories FIRST ---
        var categoryNames = new[] { "Fiction", "Science Fiction", "Mystery", "Non-Fiction" }; // Example categories
        var categoryFaker = new Faker<Category>()
            .CustomInstantiator(f => new Category(
                Guid.NewGuid(),
                f.PickRandom(categoryNames), // Use predefined names for consistency
                f.Lorem.Slug(2) // Generate a simple slug
            ))
            // Ensure unique names/slugs if needed (using DistinctBy after generation)
            .RuleFor(c => c.Slug, (f, c) => c.Name.ToLowerInvariant().Replace(" ", "-")); // Generate slug from name

        // Generate unique categories
        var categories = categoryFaker.Generate(NumberOfCategories)
                                      .GroupBy(c => c.Name) // Group by name to get unique
                                      .Select(g => g.First())
                                      .ToList();

        await catalogDbContext.Categories.AddRangeAsync(categories);
        await catalogDbContext.SaveChangesAsync(); // Save categories to get IDs
        logger.LogInformation("Seeded {Count} categories.", categories.Count);

        // --- Seed Authors ---
        var authorFaker = new Faker<Author>()
            .CustomInstantiator(f => new Author(
                Guid.NewGuid(),
                f.Name.FullName(),
                f.Lorem.Paragraph(2)
             ));

        var authors = authorFaker.Generate(NumberOfAuthors);
        await catalogDbContext.Authors.AddRangeAsync(authors);
        await catalogDbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} authors.", authors.Count);

        // --- Seed Books ---
        var bookFaker = new Faker<Book>()
            .CustomInstantiator(f =>
            {
                var author = f.PickRandom(authors);
                var category = f.PickRandom(categories); // Pick a random seeded category
                return new Book(
                    Guid.NewGuid(),
                    f.Commerce.ProductName(),
                    f.Lorem.Paragraphs(2),
                    f.Random.ReplaceNumbers("############"),
                    f.Random.Decimal(5.00m, 50.00m),
                    author.Id,
                    category.Id 
                );
            });

        var allBooks = new List<Book>();
        foreach (var author in authors) // Assign books per author for better distribution
        {
            int bookCount = Randomizer.Seed.Next(BooksPerAuthorMin, BooksPerAuthorMax + 1);
            var booksForAuthor = bookFaker.Generate(bookCount);
            // Re-assign authorId just in case PickRandom chose a different one
            // And ensure categoryId is assigned (already done in faker)
            booksForAuthor.ForEach(b => { /* Constructor assigns IDs */ });
            allBooks.AddRange(booksForAuthor);
        }
        // Ensure we didn't generate duplicate ISBNs (simple approach)
        allBooks = allBooks.DistinctBy(b => b.Isbn).ToList();


        await catalogDbContext.Books.AddRangeAsync(allBooks);
        await catalogDbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} books.", allBooks.Count);

        logger.LogInformation("Catalog data seeding completed.");
    }
}