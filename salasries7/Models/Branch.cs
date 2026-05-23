using System.ComponentModel.DataAnnotations;

namespace salasries7.Models;

public class Branch : ISyncable
{
    public int Id { get; set; }

    // Sync Fields
    public Guid SyncId { get; set; } = Guid.NewGuid();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;

    [Required(ErrorMessage = "رمز الفرع مطلوب")]
    [StringLength(20)]
    [Display(Name = "رمز الفرع")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الفرع مطلوب")]
    [StringLength(120)]
    [Display(Name = "اسم الفرع")]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "المدينة")]
    public string? City { get; set; }

    [StringLength(250)]
    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
}
