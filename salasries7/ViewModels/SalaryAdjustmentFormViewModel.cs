using Microsoft.AspNetCore.Mvc.Rendering;
using salasries7.Models;

namespace salasries7.ViewModels;

public class SalaryAdjustmentFormViewModel
{
    public SalaryAdjustment Adjustment { get; set; } = new();
    public string? BranchName { get; set; }
    public IEnumerable<SelectListItem> Employees { get; set; } = [];
}

public class SalaryAdjustmentIndexViewModel
{
    public string BranchName { get; set; } = string.Empty;
    public int Year { get; set; } = DateTime.Today.Year;
    public int Month { get; set; } = DateTime.Today.Month;
    public IReadOnlyList<SalaryAdjustment> Adjustments { get; set; } = [];

    public decimal TotalBonuses => Adjustments
        .Where(adjustment => adjustment.Type is SalaryAdjustmentType.Bonus or SalaryAdjustmentType.Overtime)
        .Sum(adjustment => adjustment.Amount);

    public decimal TotalDeductions => Adjustments
        .Where(adjustment => adjustment.Type is SalaryAdjustmentType.Deduction or SalaryAdjustmentType.Loan)
        .Sum(adjustment => adjustment.Amount);
}
