namespace Modules.Catalog.Domain.Policies;

public static class CatalogPolicyConsts
{
    // Example policy names
    public const string ReadCatalogPolicy = "catalog:read"; // Maybe allow anonymous?
    public const string ManageCatalogPolicy = "catalog:manage"; // For Create/Update/Delete
}