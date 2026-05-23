using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace salasries7.Models;

public class EmployeeLoan : ISyncable
{
    public int Id { get; set; }

    // Sync Fields
    public Guid SyncId { get; set; } = Guid.NewGuid();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;

    [Required]
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    [Required(ErrorMessage = "مبلغ السلفة مطلوب")]
    [Range(0.001, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "إجمالي مبلغ السلفة")]
    public decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "القسط الشهري مطلوب")]
    [Range(0.001, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "القسط الشهري")]
    public decimal MonthlyInstallment { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "المبلغ المتبقي")]
    public decimal RemainingAmount { get; set; }

    [Required(ErrorMessage = "تاريخ البدء مطلوب")]
    [DataType(DataType.Date)]
    [Display(Name = "تاريخ بدء الخصم")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "حالة السلفة")]
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    [StringLength(500)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    [Display(Name = "تاريخ الإنشاء")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
