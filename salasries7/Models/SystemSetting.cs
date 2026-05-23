using System.ComponentModel.DataAnnotations;

namespace salasries7.Models;

public class SystemSetting
{
    [Key]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? Category { get; set; } // Finance, Operations, UI
}
