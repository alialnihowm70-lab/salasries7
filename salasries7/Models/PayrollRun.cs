using System.ComponentModel.DataAnnotations;

namespace salasries7.Models;

public class PayrollRun : ISyncable
{
    public int Id { get; set; }

    // Sync Fields
    public Guid SyncId { get; set; } = Guid.NewGuid();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;

    [Range(2000, 2100)]
    [Display(Name = "السنة")]
    public int Year { get; set; } = DateTime.Today.Year;

    [Range(1, 12)]
    [Display(Name = "الشهر")]
    public int Month { get; set; } = DateTime.Today.Month;

    [Display(Name = "الفرع")]
    public int? BranchId { get; set; }

    public Branch? Branch { get; set; }

    [Display(Name = "الحالة")]
    public PayrollRunStatus Status { get; set; } = PayrollRunStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public ICollection<PayrollLine> Lines { get; set; } = new List<PayrollLine>();
}
