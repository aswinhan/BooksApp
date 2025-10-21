namespace Modules.Users.Infrastructure.Database;

public static class DbConsts
{
    // Defines the schema name for tables in this module
    public const string Schema = "users";

    // Defines the name for the EF Core migrations history table
    public const string MigrationTableName = "__EFMigrationsHistory"; // Standard EF Core name
}