using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace salasries7.ViewModels;

public class PayrollCreateViewModel
{
    [Range(2000, 2100)]
    [Display(Name = "السنة")]
    public int Year { get; set; } = DateTime.Today.Year;

    [Range(1, 12)]
    [Display(Name = "الشهر")]
    public int Month { get; set; } = DateTime.Today.Month;

    [Display(Name = "الفرع")]
    public int? BranchId { get; set; }

    public string? BranchName { get; set; }
    public bool IsBranchLocked { get; set; }
    public IEnumerable<SelectListItem> Branches { get; set; } = [];
}
