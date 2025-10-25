using System.Diagnostics;

namespace Modules.Inventory.Features.Tracing;

internal static class InventoryActivitySource
{
    internal static readonly ActivitySource Instance = new("Inventory");
    internal static string ActivitySourceName => Instance.Name;
}