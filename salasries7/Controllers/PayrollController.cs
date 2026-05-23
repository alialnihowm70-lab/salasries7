using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using salasries7.Services;
using salasries7.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace salasries7.Controllers;

[Authorize]
public class PayrollController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBranchContext _branchContext;
    private readonly IPayrollService _payrollService;

    public PayrollController(ApplicationDbContext db, IBranchContext branchContext, IPayrollService payrollService)
    {
        _db = db;
        _branchContext = branchContext;
        _payrollService = payrollService;
    }

    public async Task<IActionResult> Index()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var runs = await _db.PayrollRuns
            .Include(run => run.Branch)
            .Include(run => run.Lines)
            .Where(run => run.BranchId == selectedBranch.Id)
            .OrderByDescending(run => run.Year)
            .ThenByDescending(run => run.Month)
            .ThenByDescending(run => run.CreatedAt)
            .ToListAsync();

        return View(runs);
    }

    public async Task<IActionResult> Details(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var run = await _db.PayrollRuns
            .Include(item => item.Branch)
            .Include(item => item.Lines.OrderBy(line => line.Employee!.EmployeeNumber))
                .ThenInclude(line => line.Employee)
            .FirstOrDefaultAsync(item => item.Id == id && item.BranchId == selectedBranch.Id);

        return run is null ? NotFound() : View(run);
    }

    public async Task<IActionResult> Create()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new PayrollCreateViewModel
        {
            BranchId = selectedBranch.Id,
            BranchName = selectedBranch.Name,
            Branches = GetLockedBranchOptions(selectedBranch),
            IsBranchLocked = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PayrollCreateViewModel model)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        model.BranchId = selectedBranch.Id;

        if (!await _db.Employees.AnyAsync(employee => employee.BranchId == selectedBranch.Id && employee.Status == EmploymentStatus.Active))
        {
            ModelState.AddModelError(string.Empty, "لا يوجد موظفون نشطون داخل الفرع المختار.");
        }

        if (!ModelState.IsValid)
        {
            model.BranchName = selectedBranch.Name;
            model.Branches = GetLockedBranchOptions(selectedBranch);
            model.IsBranchLocked = true;
            return View(model);
        }

        var run = await _payrollService.CreateRunAsync(model.Year, model.Month, selectedBranch.Id);
        return RedirectToAction(nameof(Details), new { id = run.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Recalculate(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var run = await GetSelectedRunAsync(id, selectedBranch.Id);
        if (run is null)
        {
            return NotFound();
        }

        if (run.Status != PayrollRunStatus.Draft)
        {
            TempData["Message"] = "لا يمكن إعادة حساب تشغيل معتمد أو مدفوع";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _payrollService.RecalculateRunAsync(id, selectedBranch.Id);
        TempData["Message"] = "تمت إعادة حساب المرتب حسب آخر الحركات";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var run = await GetSelectedRunAsync(id, selectedBranch.Id);
        if (run is null)
        {
            return NotFound();
        }

        if (run.Status == PayrollRunStatus.Draft)
        {
            run.Status = PayrollRunStatus.Approved;
            run.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Message"] = "تم اعتماد تشغيل المرتب";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var run = await GetSelectedRunAsync(id, selectedBranch.Id);
        if (run is null)
        {
            return NotFound();
        }

        if (run.Status == PayrollRunStatus.Approved)
        {
            await _payrollService.FinalizeRunAsync(id, selectedBranch.Id);
            run.Status = PayrollRunStatus.Paid;
            run.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Message"] = "تم تعليم المرتب كمدفوع وتحديث أرصدة السلف";
        }
        else if (run.Status == PayrollRunStatus.Draft)
        {
            TempData["Message"] = "اعتمد التشغيل قبل تعليمه كمدفوع";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var run = await _db.PayrollRuns
            .Include(item => item.Lines)
            .FirstOrDefaultAsync(item => item.Id == id && item.BranchId == selectedBranch.Id);

        if (run is null)
        {
            return NotFound();
        }

        if (run.Status != PayrollRunStatus.Draft)
        {
            TempData["Message"] = "لا يمكن حذف تشغيل معتمد أو مدفوع";
            return RedirectToAction(nameof(Details), new { id });
        }

        _db.PayrollRuns.Remove(run);
        await _db.SaveChangesAsync();
        TempData["Message"] = "تم حذف تشغيل المرتب";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ExportToBank(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null) return RedirectToAction("Index", "Home");

        var run = await _db.PayrollRuns
            .Include(r => r.Lines)
                .ThenInclude(l => l.Employee)
            .FirstOrDefaultAsync(r => r.Id == id && r.BranchId == selectedBranch.Id);

        if (run is null) return NotFound();

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("الرقم وظيفي,اسم الموظف,البنك,رقم الحساب,IBAN,صافي المرتب");

        foreach (var line in run.Lines.OrderBy(l => l.Employee?.EmployeeNumber))
        {
            builder.AppendLine($"{line.Employee?.EmployeeNumber},{line.Employee?.FullName},{line.Employee?.BankName},{line.Employee?.AccountNumber},{line.Employee?.IBAN},{line.NetSalary:N3}");
        }

        var fileName = $"Payroll_Bank_{run.Year}_{run.Month}_{selectedBranch.Name}.csv";
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private static IReadOnlyList<SelectListItem> GetLockedBranchOptions(Branch selectedBranch)
    {
        return
        [
            new SelectListItem(selectedBranch.Name, selectedBranch.Id.ToString(), selected: true)
        ];
    }

    private async Task<PayrollRun?> GetSelectedRunAsync(int id, int branchId)
    {
        return await _db.PayrollRuns
            .FirstOrDefaultAsync(item => item.Id == id && item.BranchId == branchId);
    }
}
