using Bogus;
using Microsoft.AspNetCore.Identity; // Need UserManager
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Domain.Entities; // Need Post
using Modules.Blog.Infrastructure.Database; // Need BlogDbContext
using Modules.Users.Domain.Users; // Need User
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksApp.Host.Seeding; // Corrected namespace

public class BlogSeedService(
    BlogDbContext blogDbContext,
    UserManager<User> userManager, // Inject UserManager to get Authors
    ILogger<BlogSeedService> logger)
{
    private const int NumberOfPosts = 15;

    public async Task SeedBlogAsync()
    {
        Randomizer.Seed = new Random(67890);

        if (await blogDbContext.Posts.AnyAsync())
        {
            logger.LogInformation("Blog data already exists, skipping seeding.");
            return;
        }

        // Get existing users to assign as authors
        // Fetching only a few users for seeding purposes
        var authors = await userManager.Users.Take(5).ToListAsync();
        if (authors.Count == 0)
        {
            logger.LogWarning("No users found in database. Cannot seed blog posts without authors. Run User seeding first.");
            return; // Can't proceed without authors
        }

        logger.LogInformation("Starting Blog data seeding...");

        // --- Seed Posts ---
        var postFaker = new Faker<Post>()
            .CustomInstantiator(f =>
            {
                var author = f.PickRandom(authors);
                var title = f.Lorem.Sentence(5, 5); // 5-10 words
                var content = f.Lorem.Paragraphs(f.Random.Int(3, 7));
                // Simple slug generation for seeding
                var slug = title.ToLowerInvariant().Replace(" ", "-").Replace(".", "").Replace(",", "");

                return new Post(
                    Guid.NewGuid(),
                    title,
                    content,
                    author.Id,
                    author.DisplayName ?? author.UserName!, // Use DisplayName or UserName
                    slug
                );
            })
            // Optionally publish some posts
            .FinishWith((f, post) =>
            {
                if (f.Random.Bool(0.7f)) // 70% chance of being published
                {
                    post.Publish();
                }
            });


        var posts = postFaker.Generate(NumberOfPosts);

        // Handle potential duplicate slugs during generation (simple approach: append index)
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
            // If slug was modified, update the post object (need setter or different approach)
            // For simplicity, we'll assume constructor sets it and check *before* adding.
            // A better way is to regenerate slug in handler if conflict.
            // Let's assume generated slugs are unique enough for seeding here.
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
}