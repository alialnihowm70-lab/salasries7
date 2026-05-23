using System.ComponentModel.DataAnnotations.Schema;

namespace salasries7.Models;

public class PayrollLine : ISyncable
{
    public int Id { get; set; }

    // Sync Fields
    public Guid SyncId { get; set; } = Guid.NewGuid();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;

    public int PayrollRunId { get; set; }
    public PayrollRun? PayrollRun { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal BasicSalary { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal TotalAllowances { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal OvertimePay { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal Bonuses { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal GrossSalary { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal SocialSecurityDeduction { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal TaxDeduction { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal OtherDeductions { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal NetSalary { get; set; }

    public string? Notes { get; set; }
}
