using System.Diagnostics;

namespace Modules.Discounts.Features.Tracing;

internal static class DiscountsActivitySource
{
    internal static readonly ActivitySource Instance = new("Discounts");
    internal static string ActivitySourceName => Instance.Name;
}