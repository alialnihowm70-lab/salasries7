using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using salasries7.Services;

namespace salasries7.Controllers;

public class LoansController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBranchContext _branchContext;

    public LoansController(ApplicationDbContext db, IBranchContext branchContext)
    {
        _db = db;
        _branchContext = branchContext;
    }

    public async Task<IActionResult> Index()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null) return RedirectToAction("Index", "Home");

        var loans = await _db.EmployeeLoans
            .Include(l => l.Employee)
            .Where(l => l.Employee.BranchId == selectedBranch.Id)
            .OrderByDescending(l => l.CreatedDate)
            .ToListAsync();

        return View(loans);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int employeeId, decimal totalAmount, decimal monthlyInstallment, DateTime startDate, string? notes)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null) return RedirectToAction("Index", "Home");

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.BranchId == selectedBranch.Id);
        if (employee is null) return NotFound();

        var loan = new EmployeeLoan
        {
            EmployeeId = employeeId,
            TotalAmount = totalAmount,
            MonthlyInstallment = monthlyInstallment,
            RemainingAmount = totalAmount,
            StartDate = startDate,
            Notes = notes,
            Status = LoanStatus.Active,
            CreatedDate = DateTime.Now
        };

        _db.EmployeeLoans.Add(loan);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Employees", new { id = employeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Terminate(int id)
    {
        var loan = await _db.EmployeeLoans.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
        if (loan is null) return NotFound();

        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null || loan.Employee?.BranchId != selectedBranch.Id) return Forbid();

        loan.Status = LoanStatus.PaidOff;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
