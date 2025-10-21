using System.Reflection;

namespace Modules.Orders.Infrastructure;

// Standard marker class for referencing this assembly
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}