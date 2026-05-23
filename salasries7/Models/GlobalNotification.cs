using System.ComponentModel.DataAnnotations;

namespace salasries7.Models;

public enum NotificationSeverity { Info, Warning, Danger, Success }

public class GlobalNotification
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsRead { get; set; } = false;
    
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
    
    public string? ActionUrl { get; set; }
}
