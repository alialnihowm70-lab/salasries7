using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace salasries7.Models;

public class AttendanceRecord : ISyncable
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
    [Display(Name = "التاريخ")]
    public DateTime WorkDate { get; set; } = DateTime.Today;

    [Display(Name = "الحالة")]
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    [DataType(DataType.Time)]
    [Display(Name = "وقت الدخول")]
    public TimeSpan? CheckIn { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "وقت الخروج")]
    public TimeSpan? CheckOut { get; set; }

    [Range(0, 1440)]
    [Display(Name = "دقائق التأخير")]
    public int LateMinutes { get; set; }

    [Range(0, 24)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "ساعات إضافية")]
    public decimal OvertimeHours { get; set; }

    [StringLength(300)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
}
