using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using salasries7.Services;
using salasries7.ViewModels;

namespace salasries7.Controllers;

public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBranchContext _branchContext;

    public AttendanceController(ApplicationDbContext db, IBranchContext branchContext)
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

        var records = await _db.AttendanceRecords
            .Include(record => record.Employee)
            .Where(record => record.Employee!.BranchId == selectedBranch.Id
                && record.WorkDate >= start
                && record.WorkDate < end)
            .OrderByDescending(record => record.WorkDate)
            .ThenBy(record => record.Employee!.EmployeeNumber)
            .ToListAsync();

        return View(new AttendanceIndexViewModel
        {
            BranchName = selectedBranch.Name,
            Year = selectedYear,
            Month = selectedMonth,
            Records = records
        });
    }

    public async Task<IActionResult> Create()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(await BuildFormModelAsync(selectedBranch, new AttendanceRecord
        {
            WorkDate = DateTime.Today,
            Status = AttendanceStatus.Present
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AttendanceFormViewModel model)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        await ValidateRecordAsync(model.Record, selectedBranch.Id);

        if (!ModelState.IsValid)
        {
            return View(await BuildFormModelAsync(selectedBranch, model.Record));
        }

        model.Record.WorkDate = model.Record.WorkDate.Date;
        _db.AttendanceRecords.Add(model.Record);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = model.Record.WorkDate.Year, month = model.Record.WorkDate.Month });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var record = await GetBranchRecordAsync(id, selectedBranch.Id);
        return record is null ? NotFound() : View(await BuildFormModelAsync(selectedBranch, record));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AttendanceFormViewModel model)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (id != model.Record.Id)
        {
            return NotFound();
        }

        var record = await GetBranchRecordAsync(id, selectedBranch.Id);
        if (record is null)
        {
            return NotFound();
        }

        await ValidateRecordAsync(model.Record, selectedBranch.Id);

        if (!ModelState.IsValid)
        {
            return View(await BuildFormModelAsync(selectedBranch, model.Record));
        }

        model.Record.WorkDate = model.Record.WorkDate.Date;
        record.EmployeeId = model.Record.EmployeeId;
        record.WorkDate = model.Record.WorkDate;
        record.Status = model.Record.Status;
        record.CheckIn = model.Record.CheckIn;
        record.CheckOut = model.Record.CheckOut;
        record.LateMinutes = model.Record.LateMinutes;
        record.OvertimeHours = model.Record.OvertimeHours;
        record.Notes = model.Record.Notes;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = model.Record.WorkDate.Year, month = model.Record.WorkDate.Month });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var record = await GetBranchRecordAsync(id, selectedBranch.Id);
        return record is null ? NotFound() : View(record);
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

        var record = await GetBranchRecordAsync(id, selectedBranch.Id);
        if (record is null)
        {
            return NotFound();
        }

        _db.AttendanceRecords.Remove(record);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = record.WorkDate.Year, month = record.WorkDate.Month });
    }

    private async Task<AttendanceFormViewModel> BuildFormModelAsync(Branch selectedBranch, AttendanceRecord record)
    {
        return new AttendanceFormViewModel
        {
            BranchName = selectedBranch.Name,
            Record = record,
            Employees = await GetEmployeeOptionsAsync(selectedBranch.Id, record.EmployeeId)
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

    private async Task<AttendanceRecord?> GetBranchRecordAsync(int id, int branchId)
    {
        return await _db.AttendanceRecords
            .Include(record => record.Employee)
            .FirstOrDefaultAsync(record => record.Id == id && record.Employee!.BranchId == branchId);
    }

    private async Task ValidateRecordAsync(AttendanceRecord record, int branchId)
    {
        var employeeExists = await _db.Employees
            .AnyAsync(employee => employee.Id == record.EmployeeId && employee.BranchId == branchId);

        if (!employeeExists)
        {
            ModelState.AddModelError("Record.EmployeeId", "اختر موظفًا من الفرع الحالي");
        }

        var duplicateExists = await _db.AttendanceRecords
            .AnyAsync(item => item.Id != record.Id
                && item.EmployeeId == record.EmployeeId
                && item.WorkDate == record.WorkDate.Date);

        if (duplicateExists)
        {
            ModelState.AddModelError("Record.WorkDate", "تم تسجيل حضور هذا الموظف في نفس التاريخ");
        }
    }
}
