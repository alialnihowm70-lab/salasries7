using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using salasries7.Services;
using salasries7.ViewModels;

namespace salasries7.Controllers;

public class SalaryAdjustmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBranchContext _branchContext;

    public SalaryAdjustmentsController(ApplicationDbContext db, IBranchContext branchContext)
    {
        _db = db;
        _branchContext = branchContext;
    }

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var selectedYear = year ?? DateTime.Today.Year;
        var selectedMonth = month ?? DateTime.Today.Month;
        var start = new DateTime(selectedYear, selectedMonth, 1);
        var end = start.AddMonths(1);

        var adjustments = await _db.SalaryAdjustments
            .Include(adjustment => adjustment.Employee)
            .Where(adjustment => adjustment.Employee!.BranchId == selectedBranch.Id
                && adjustment.Month >= start
                && adjustment.Month < end)
            .OrderBy(adjustment => adjustment.Employee!.EmployeeNumber)
            .ThenBy(adjustment => adjustment.Type)
            .ToListAsync();

        return View(new SalaryAdjustmentIndexViewModel
        {
            BranchName = selectedBranch.Name,
            Year = selectedYear,
            Month = selectedMonth,
            Adjustments = adjustments
        });
    }

    public async Task<IActionResult> Create()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(await BuildFormModelAsync(selectedBranch, new SalaryAdjustment
        {
            Month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
            Type = SalaryAdjustmentType.Bonus
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SalaryAdjustmentFormViewModel model)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        await ValidateAdjustmentAsync(model.Adjustment, selectedBranch.Id);

        if (!ModelState.IsValid)
        {
            return View(await BuildFormModelAsync(selectedBranch, model.Adjustment));
        }

        model.Adjustment.Month = FirstDayOfMonth(model.Adjustment.Month);
        _db.SalaryAdjustments.Add(model.Adjustment);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = model.Adjustment.Month.Year, month = model.Adjustment.Month.Month });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var adjustment = await GetBranchAdjustmentAsync(id, selectedBranch.Id);
        return adjustment is null ? NotFound() : View(await BuildFormModelAsync(selectedBranch, adjustment));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SalaryAdjustmentFormViewModel model)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (id != model.Adjustment.Id)
        {
            return NotFound();
        }

        var adjustment = await GetBranchAdjustmentAsync(id, selectedBranch.Id);
        if (adjustment is null)
        {
            return NotFound();
        }

        await ValidateAdjustmentAsync(model.Adjustment, selectedBranch.Id);

        if (!ModelState.IsValid)
        {
            return View(await BuildFormModelAsync(selectedBranch, model.Adjustment));
        }

        model.Adjustment.Month = FirstDayOfMonth(model.Adjustment.Month);
        adjustment.EmployeeId = model.Adjustment.EmployeeId;
        adjustment.Month = model.Adjustment.Month;
        adjustment.Type = model.Adjustment.Type;
        adjustment.Title = model.Adjustment.Title;
        adjustment.Amount = model.Adjustment.Amount;
        adjustment.Notes = model.Adjustment.Notes;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = adjustment.Month.Year, month = adjustment.Month.Month });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var adjustment = await GetBranchAdjustmentAsync(id, selectedBranch.Id);
        return adjustment is null ? NotFound() : View(adjustment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var adjustment = await GetBranchAdjustmentAsync(id, selectedBranch.Id);
        if (adjustment is null)
        {
            return NotFound();
        }

        _db.SalaryAdjustments.Remove(adjustment);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = adjustment.Month.Year, month = adjustment.Month.Month });
    }

    private async Task<SalaryAdjustmentFormViewModel> BuildFormModelAsync(Branch selectedBranch, SalaryAdjustment adjustment)
    {
        return new SalaryAdjustmentFormViewModel
        {
            BranchName = selectedBranch.Name,
            Adjustment = adjustment,
            Employees = await GetEmployeeOptionsAsync(selectedBranch.Id, adjustment.EmployeeId)
        };
    }

    private async Task<IReadOnlyList<SelectListItem>> GetEmployeeOptionsAsync(int branchId, int selectedEmployeeId)
    {
        var employees = await _db.Employees
            .AsNoTracking()
            .Where(employee => employee.BranchId == branchId && employee.Status == EmploymentStatus.Active)
            .OrderBy(employee => employee.EmployeeNumber)
            .ToListAsync();

        return employees
            .Select(employee => new SelectListItem(
                $"{employee.EmployeeNumber} - {employee.FullName}",
                employee.Id.ToString(),
                employee.Id == selectedEmployeeId))
            .ToList();
    }

    private async Task<SalaryAdjustment?> GetBranchAdjustmentAsync(int id, int branchId)
    {
        return await _db.SalaryAdjustments
            .Include(adjustment => adjustment.Employee)
            .FirstOrDefaultAsync(adjustment => adjustment.Id == id && adjustment.Employee!.BranchId == branchId);
    }

    private async Task ValidateAdjustmentAsync(SalaryAdjustment adjustment, int branchId)
    {
        var employeeExists = await _db.Employees
            .AnyAsync(employee => employee.Id == adjustment.EmployeeId && employee.BranchId == branchId);

        if (!employeeExists)
        {
            ModelState.AddModelError("Adjustment.EmployeeId", "اختر موظفًا من الفرع الحالي");
        }
    }

    private static DateTime FirstDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }
}
