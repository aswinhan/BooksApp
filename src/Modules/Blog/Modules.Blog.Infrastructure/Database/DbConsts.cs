namespace Modules.Blog.Infrastructure.Database;

public static class DbConsts
{
    // Schema name for blog-related tables
    public const string Schema = "blog";

    // Standard EF Core migration history table name
    public const string MigrationTableName = "__EFMigrationsHistory";
}