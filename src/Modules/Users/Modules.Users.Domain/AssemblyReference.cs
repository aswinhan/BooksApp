using System.Reflection;

namespace Modules.Users.Domain;

// Standard marker class for referencing this assembly
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}