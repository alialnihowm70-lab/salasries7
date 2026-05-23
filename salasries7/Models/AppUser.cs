using System.ComponentModel.DataAnnotations;

namespace salasries7.Models;

public enum UserRole { Admin, HR, Accounting, Employee }

public class AppUser
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.Employee;
    
    // Optional: link to an actual employee profile
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
