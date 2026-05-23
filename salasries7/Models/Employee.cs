using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace salasries7.Models;

public class Employee : ISyncable
{
    public int Id { get; set; }

    // Sync Fields
    public Guid SyncId { get; set; } = Guid.NewGuid();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;

    [Required(ErrorMessage = "رقم الموظف مطلوب")]
    [StringLength(30)]
    [Display(Name = "رقم الموظف")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الموظف مطلوب")]
    [StringLength(140)]
    [Display(Name = "اسم الموظف")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(30)]
    [Display(Name = "الرقم الوطني")]
    public string? NationalId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ انتهاء الرقم الوطني")]
    public DateTime? NationalIdExpiry { get; set; }

    [StringLength(50)]
    [Display(Name = "رقم الجواز")]
    public string? PassportNumber { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ انتهاء الجواز")]
    public DateTime? PassportExpiry { get; set; }

    [StringLength(100)]
    [Display(Name = "المسمى الوظيفي")]
    public string? JobTitle { get; set; }

    [StringLength(100)]
    [Display(Name = "القسم")]
    public string? Department { get; set; }

    [Display(Name = "الفرع")]
    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ التعيين")]
    public DateTime HireDate { get; set; } = DateTime.Today;

    [Display(Name = "الحالة")]
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

    // Contact Information
    [Phone]
    [StringLength(20)]
    [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [StringLength(100)]
    [Display(Name = "البريد الإلكتروني")]
    public string? PersonalEmail { get; set; }

    [StringLength(250)]
    [Display(Name = "العنوان الكامل")]
    public string? FullAddress { get; set; }

    // Financial Information
    [Range(0, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "المرتب الأساسي")]
    public decimal BasicSalary { get; set; }

    [Range(0, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "بدل السكن")]
    public decimal HousingAllowance { get; set; }

    [Range(0, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "بدل المواصلات")]
    public decimal TransportationAllowance { get; set; }

    [Range(0, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "بدلات أخرى")]
    public decimal OtherAllowance { get; set; }

    [Range(0, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "خصم الضمان")]
    public decimal SocialSecurityDeduction { get; set; }

    [Range(0, 9999999)]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "ضريبة الدخل")]
    public decimal TaxDeduction { get; set; }

    // Bank Information
    [StringLength(100)]
    [Display(Name = "اسم البنك")]
    public string? BankName { get; set; }

    [StringLength(50)]
    [Display(Name = "رقم الحساب")]
    public string? AccountNumber { get; set; }

    [StringLength(50)]
    [Display(Name = "IBAN")]
    public string? IBAN { get; set; }

    [StringLength(500)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<SalaryAdjustment> SalaryAdjustments { get; set; } = new List<SalaryAdjustment>();
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeLoan> Loans { get; set; } = new List<EmployeeLoan>();
    public ICollection<EmployeeLeave> Leaves { get; set; } = new List<EmployeeLeave>();

    [NotMapped]
    [Display(Name = "إجمالي البدلات")]
    public decimal TotalAllowances => HousingAllowance + TransportationAllowance + OtherAllowance;

    [NotMapped]
    [Display(Name = "الإجمالي الشهري")]
    public decimal MonthlyGrossSalary => BasicSalary + TotalAllowances;
}
