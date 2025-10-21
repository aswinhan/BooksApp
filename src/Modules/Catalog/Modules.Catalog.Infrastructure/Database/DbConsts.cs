namespace Modules.Catalog.Infrastructure.Database;

public static class DbConsts
{
    // Schema name for catalog-related tables
    public const string Schema = "catalog";

    // Standard EF Core migration history table name
    public const string MigrationTableName = "__EFMigrationsHistory";
}