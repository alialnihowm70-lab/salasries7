using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using System.Globalization;

namespace salasries7.Controllers;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Monthly Payroll Totals (Last 6 Months)
        var last6Months = Enumerable.Range(0, 6)
            .Select(i => DateTime.Today.AddMonths(-i))
            .OrderBy(d => d)
            .ToList();

        var monthlyTrends = new List<object>();
        foreach (var date in last6Months)
        {
            var total = await _db.PayrollLines
                .Where(l => l.PayrollRun.Year == date.Year && l.PayrollRun.Month == date.Month)
                .SumAsync(l => l.GrossSalary);
            
            monthlyTrends.Add(new { Month = date.ToString("MMMM yyyy", new CultureInfo("ar-LY")), Total = total });
        }

        // 2. Branch Distribution (Employee Count)
        var branchStats = await _db.Branches
            .Select(b => new { 
                Name = b.Name, 
                Count = b.Employees.Count(e => e.Status == EmploymentStatus.Active),
                TotalPayroll = b.Employees.Where(e => e.Status == EmploymentStatus.Active).Sum(e => e.BasicSalary + e.HousingAllowance + e.TransportationAllowance + e.OtherAllowance)
            })
            .ToListAsync();

        // 3. Salary Component Distribution (Global)
        var activeEmployees = await _db.Employees.Where(e => e.Status == EmploymentStatus.Active).ToListAsync();
        var components = new
        {
            Basic = activeEmployees.Sum(e => e.BasicSalary),
            Allowances = activeEmployees.Sum(e => e.TotalAllowances),
            Deductions = activeEmployees.Sum(e => e.SocialSecurityDeduction + e.TaxDeduction)
        };

        // 4. Operational KPIs
        ViewBag.TotalEmployees = activeEmployees.Count;
        ViewBag.TotalMonthlyPayroll = activeEmployees.Sum(e => e.MonthlyGrossSalary);
        ViewBag.AverageSalary = activeEmployees.Any() ? activeEmployees.Average(e => e.MonthlyGrossSalary) : 0;

        ViewBag.MonthlyTrends = monthlyTrends;
        ViewBag.BranchStats = branchStats;
        ViewBag.Components = components;

        return View();
    }

    public async Task<IActionResult> Financial()
    {
        var activeEmployees = await _db.Employees
            .Include(e => e.Branch)
            .Where(e => e.Status == EmploymentStatus.Active)
            .ToListAsync();

        return View(activeEmployees);
    }

    public async Task<IActionResult> Operational()
    {
        var leaves = await _db.EmployeeLeaves
            .Include(e => e.Employee)
            .OrderByDescending(e => e.StartDate)
            .Take(50)
            .ToListAsync();

        var attendanceSummary = await _db.AttendanceRecords
            .GroupBy(a => a.WorkDate)
            .OrderByDescending(g => g.Key)
            .Take(30)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        ViewBag.AttendanceSummary = attendanceSummary;
        return View(leaves);
    }
}
