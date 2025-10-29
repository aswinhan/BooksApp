using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Domain.Entities; // Need Post, BlogCategory, Tag
using Modules.Blog.Infrastructure.Database; // Need BlogDbContext
using Modules.Users.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksApp.Host.Seeding;

public class BlogSeedService(
    BlogDbContext blogDbContext,
    UserManager<User> userManager,
    ILogger<BlogSeedService> logger)
{
    private const int NumberOfPosts = 15;
    private const int NumberOfBlogCategories = 4; // Add category count

    public async Task SeedBlogAsync()
    {
        Randomizer.Seed = new Random(67890);

        if (await blogDbContext.Posts.AnyAsync())
        {
            logger.LogInformation("Blog data already exists, skipping seeding.");
            return;
        }

        // --- 1. Get or Seed Blog Categories ---
        var categories = await SeedBlogCategoriesAsync();
        if (categories.Count == 0)
        {
            logger.LogWarning("No blog categories found or created. Cannot seed posts without categories.");
            return; // Stop if no categories
        }

        // --- 2. Get Users (Authors) ---
        var authors = await userManager.Users.Take(5).ToListAsync();
        if (authors.Count == 0)
        {
            logger.LogWarning("No users found in database. Cannot seed blog posts without authors. Run User seeding first.");
            return;
        }

        logger.LogInformation("Starting Blog data seeding...");

        // --- 3. Seed Posts ---
        var postFaker = new Faker<Post>()
            .CustomInstantiator(f =>
            {
                var author = f.PickRandom(authors);
                var category = f.PickRandom(categories); // Pick a random category
                var title = f.Lorem.Sentence(5, 5);
                var content = f.Lorem.Paragraphs(f.Random.Int(3, 7));
                var slug = title.ToLowerInvariant().Replace(" ", "-").Replace(".", "").Replace(",", "");

                // --- PASS category.Id to the constructor ---
                return new Post(
                    Guid.NewGuid(),
                    title,
                    content,
                    author.Id,
                    author.DisplayName ?? author.UserName!,
                    slug,
                    category.Id // <-- PROVIDE THE CATEGORY ID
                );
            })
            .FinishWith((f, post) =>
            {
                if (f.Random.Bool(0.7f))
                {
                    post.Publish();
                }
            });


        var posts = postFaker.Generate(NumberOfPosts);

        // Handle potential duplicate slugs
        var uniquePosts = new List<Post>();
        var usedSlugs = new HashSet<string>();
        int duplicateCounter = 0;
        foreach (var post in posts)
        {
            string originalSlug = post.Slug;
            string currentSlug = post.Slug;
            while (usedSlugs.Contains(currentSlug))
            {
                duplicateCounter++;
                currentSlug = $"{originalSlug}-{duplicateCounter}";
            }
            // Need to update slug if it changed - requires public setter or update method
            // post.SetSlug(currentSlug); // Assumes a method like this exists if slug changed
            // For simplicity, we assume generated slugs are unique enough for seeding here.
            if (!usedSlugs.Contains(post.Slug))
            {
                uniquePosts.Add(post);
                usedSlugs.Add(post.Slug);
            }
            else
            {
                logger.LogWarning("Skipping post due to potential duplicate slug during seeding: {Slug}", post.Slug);
            }
        }


        await blogDbContext.Posts.AddRangeAsync(uniquePosts);
        await blogDbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} posts.", uniquePosts.Count);

        logger.LogInformation("Blog data seeding completed.");
    }

    // --- ADDED: Helper method to seed categories ---
    private async Task<List<BlogCategory>> SeedBlogCategoriesAsync()
    {
        if (await blogDbContext.BlogCategories.AnyAsync())
        {
            return await blogDbContext.BlogCategories.ToListAsync(); // Return existing
        }

        logger.LogInformation("Seeding blog categories...");
        var categoryNames = new[] { "Tutorials", "News", "Opinion", "Reviews" };
        var categoryFaker = new Faker<BlogCategory>()
            .CustomInstantiator(f =>
            {
                var name = f.PickRandom(categoryNames);
                var slug = name.ToLowerInvariant().Replace(" ", "-");
                return new BlogCategory(Guid.NewGuid(), name, slug);
            });

        var categories = categoryFaker.Generate(NumberOfBlogCategories)
                                     .GroupBy(c => c.Name).Select(g => g.First()) // Ensure unique names
                                     .ToList();

        await blogDbContext.BlogCategories.AddRangeAsync(categories);
        await blogDbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} blog categories.", categories.Count);
        return categories;
    }
}