using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using salasries7.Services;

namespace salasries7.Controllers;

public class LeavesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBranchContext _branchContext;
    private readonly IAuditService _audit;

    public LeavesController(ApplicationDbContext db, IBranchContext branchContext, IAuditService audit)
    {
        _db = db;
        _branchContext = branchContext;
        _audit = audit;
    }

    public async Task<IActionResult> Index()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null) return RedirectToAction("Index", "Home");

        var leaves = await _db.EmployeeLeaves
            .Include(l => l.Employee)
            .Where(l => l.Employee.BranchId == selectedBranch.Id)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();

        return View(leaves);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int employeeId, LeaveType type, DateTime startDate, DateTime endDate, string? reason)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null) return RedirectToAction("Index", "Home");

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.BranchId == selectedBranch.Id);
        if (employee is null) return NotFound();

        var leave = new EmployeeLeave
        {
            EmployeeId = employeeId,
            Type = type,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason,
            CreatedDate = DateTime.Now,
            Status = LeaveStatus.Approved // Auto-approve for now
        };

        _db.EmployeeLeaves.Add(leave);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("CreateLeave", "EmployeeLeave", leave.Id.ToString(), $"Employee: {employee.FullName}, Type: {type}, From: {startDate:d}, To: {endDate:d}");

        return RedirectToAction("Details", "Employees", new { id = employeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var leave = await _db.EmployeeLeaves.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
        if (leave is null) return NotFound();

        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null || leave.Employee?.BranchId != selectedBranch.Id) return Forbid();

        _db.EmployeeLeaves.Remove(leave);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("DeleteLeave", "EmployeeLeave", id.ToString(), $"Employee: {leave.Employee?.FullName}, Range: {leave.StartDate:d} - {leave.EndDate:d}");

        return RedirectToAction(nameof(Index));
    }
}
