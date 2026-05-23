using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace salasries7.Models;

public class SalaryAdjustment : ISyncable
{
    public int Id { get; set; }

    // Sync Fields
    public Guid SyncId { get; set; } = Guid.NewGuid();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;

    [Display(Name = "الموظف")]
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "الشهر")]
    public DateTime Month { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    [Display(Name = "النوع")]
    public SalaryAdjustmentType Type { get; set; } = SalaryAdjustmentType.Bonus;

    [Required(ErrorMessage = "وصف الحركة مطلوب")]
    [StringLength(140)]
    [Display(Name = "الوصف")]
    public string Title { get; set; } = string.Empty;

    [Range(0, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "القيمة")]
    public decimal Amount { get; set; }

    [StringLength(300)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
