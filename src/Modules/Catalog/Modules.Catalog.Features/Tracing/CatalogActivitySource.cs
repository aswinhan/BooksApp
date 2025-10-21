namespace Modules.Catalog.Features.Tracing
{
    internal static class CatalogActivitySource
    {
        internal static readonly System.Diagnostics.ActivitySource Instance = new("Catalog"); // Define ActivitySource
    }
}