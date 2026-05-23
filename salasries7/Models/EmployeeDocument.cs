using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace salasries7.Models;

public class EmployeeDocument
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    [Required(ErrorMessage = "نوع المستند مطلوب")]
    [Display(Name = "نوع المستند")]
    public DocumentType Type { get; set; }

    [Required(ErrorMessage = "اسم الملف مطلوب")]
    [StringLength(255)]
    [Display(Name = "اسم الملف")]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "رقم المستند")]
    public string? DocumentNumber { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ الانتهاء")]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "تاريخ الرفع")]
    public DateTime UploadDate { get; set; } = DateTime.Now;

    [StringLength(500)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    [NotMapped]
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.Today;
    
    [NotMapped]
    public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value.Date >= DateTime.Today && ExpiryDate.Value.Date <= DateTime.Today.AddDays(30);
}
