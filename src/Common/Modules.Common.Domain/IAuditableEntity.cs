namespace Modules.Common.Domain;

public interface IAuditableEntity
{
    // Changed to 'get; set;' to allow the interceptor to set them.
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
}