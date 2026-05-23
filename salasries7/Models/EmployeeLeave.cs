using System.ComponentModel.DataAnnotations;

namespace salasries7.Models;

public class EmployeeLeave : ISyncable
{
    public int Id { get; set; }

    // Sync Fields
    public Guid SyncId { get; set; } = Guid.NewGuid();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required]
    [Display(Name = "نوع الإجازة")]
    public LeaveType Type { get; set; }

    [Required]
    [Display(Name = "تاريخ البدء")]
    public DateTime StartDate { get; set; }

    [Required]
    [Display(Name = "تاريخ الانتهاء")]
    public DateTime EndDate { get; set; }

    [Display(Name = "عدد الأيام")]
    public int DurationDays => (EndDate - StartDate).Days + 1;

    [Display(Name = "السبب / ملاحظات")]
    public string? Reason { get; set; }

    [Display(Name = "تاريخ التسجيل")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "الحالة")]
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
}
