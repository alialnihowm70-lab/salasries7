using System.ComponentModel.DataAnnotations;

namespace salasries7.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    public string Action { get; set; } = string.Empty;

    [Required]
    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    [Required]
    public string ModifiedBy { get; set; } = "System";

    public string? Changes { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public string? IPAddress { get; set; }
}
