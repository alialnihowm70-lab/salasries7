using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using System.Security.Claims;

namespace salasries7.Controllers;

[Authorize]
public class PortalController : Controller
{
    private readonly ApplicationDbContext _db;

    public PortalController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userIdStr = User.FindFirstValue("UserId");
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

        var userId = int.Parse(userIdStr);
        var user = await _db.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.EmployeeId == null)
        {
            // If it's an admin without an employee record, show a summary or redirect
            if (user?.Role == UserRole.Admin) return RedirectToAction("Index", "Home");
            return Content("لا يوجد سجل موظف مرتبط بهذا الحساب.");
        }

        var employeeId = user.EmployeeId.Value;
        
        // Fetch personal stats
        ViewBag.LastPay = await _db.PayrollLines
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.Id)
            .FirstOrDefaultAsync();

        ViewBag.ActiveLoans = await _db.EmployeeLoans
            .Where(l => l.EmployeeId == employeeId && l.Status == LoanStatus.Active)
            .ToListAsync();

        ViewBag.PendingLeaves = await _db.EmployeeLeaves
            .Where(l => l.EmployeeId == employeeId && l.Status == LeaveStatus.Pending)
            .CountAsync();

        return View(user.Employee);
    }

    public async Task<IActionResult> MySlips()
    {
        var userId = int.Parse(User.FindFirstValue("UserId")!);
        var user = await _db.Users.FindAsync(userId);
        if (user?.EmployeeId == null) return NotFound();

        var slips = await _db.PayrollLines
            .Include(l => l.PayrollRun)
            .Where(l => l.EmployeeId == user.EmployeeId)
            .OrderByDescending(l => l.PayrollRun.Year)
            .ThenByDescending(l => l.PayrollRun.Month)
            .ToListAsync();

        return View(slips);
    }
}
