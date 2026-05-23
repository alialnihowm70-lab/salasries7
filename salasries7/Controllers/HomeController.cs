using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using salasries7.Services;
using salasries7.ViewModels;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace salasries7.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBranchContext _branchContext;

        public HomeController(ApplicationDbContext db, IBranchContext branchContext)
        {
            _db = db;
            _branchContext = branchContext;
        }

        public async Task<IActionResult> Index()
        {
            var activeBranches = await _branchContext.GetActiveBranchesAsync();
            var selectedBranch = await _branchContext.GetSelectedBranchAsync();

            if (selectedBranch is null)
            {
                return View(new DashboardViewModel
                {
                    AvailableBranches = activeBranches,
                    BranchCount = activeBranches.Count
                });
            }

            var branchId = selectedBranch.Id;
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var model = new DashboardViewModel
            {
                SelectedBranch = selectedBranch,
                AvailableBranches = activeBranches,
                BranchCount = activeBranches.Count,
                EmployeeCount = await _db.Employees.CountAsync(employee => employee.BranchId == branchId),
                ActiveEmployeeCount = await _db.Employees.CountAsync(employee => employee.BranchId == branchId && employee.Status == EmploymentStatus.Active),
                TodayPresentCount = await _db.AttendanceRecords
                    .CountAsync(record => record.Employee!.BranchId == branchId
                        && record.WorkDate == today
                        && record.Status == AttendanceStatus.Present),
                TodayAbsentCount = await _db.AttendanceRecords
                    .CountAsync(record => record.Employee!.BranchId == branchId
                        && record.WorkDate == today
                        && record.Status == AttendanceStatus.Absent),
                MonthlyGrossPayroll = await _db.Employees
                    .Where(employee => employee.BranchId == branchId && employee.Status == EmploymentStatus.Active)
                    .SumAsync(employee => employee.BasicSalary + employee.HousingAllowance + employee.TransportationAllowance + employee.OtherAllowance),
                CurrentMonthAdditions = await _db.SalaryAdjustments
                    .Where(adjustment => adjustment.Employee!.BranchId == branchId
                        && adjustment.Month >= monthStart
                        && adjustment.Month < monthEnd
                        && (adjustment.Type == SalaryAdjustmentType.Bonus || adjustment.Type == SalaryAdjustmentType.Overtime))
                    .SumAsync(adjustment => adjustment.Amount),
                CurrentMonthDeductions = await _db.SalaryAdjustments
                    .Where(adjustment => adjustment.Employee!.BranchId == branchId
                        && adjustment.Month >= monthStart
                        && adjustment.Month < monthEnd
                        && (adjustment.Type == SalaryAdjustmentType.Deduction || adjustment.Type == SalaryAdjustmentType.Loan))
                    .SumAsync(adjustment => adjustment.Amount),
                LastPayrollRun = await _db.PayrollRuns
                    .Include(run => run.Branch)
                    .Include(run => run.Lines)
                    .Where(run => run.BranchId == branchId)
                    .OrderByDescending(run => run.CreatedAt)
                    .FirstOrDefaultAsync(),
                Branches =
                [
                    new BranchPayrollSummary
                    {
                        BranchId = selectedBranch.Id,
                        BranchName = selectedBranch.Name,
                        EmployeeCount = await _db.Employees.CountAsync(employee => employee.BranchId == branchId && employee.Status == EmploymentStatus.Active),
                        MonthlyGrossPayroll = await _db.Employees
                            .Where(employee => employee.BranchId == branchId && employee.Status == EmploymentStatus.Active)
                            .SumAsync(employee => employee.BasicSalary + employee.HousingAllowance + employee.TransportationAllowance + employee.OtherAllowance)
                    }
                ],
                RecentEmployees = await _db.Employees
                    .Include(employee => employee.Branch)
                    .Where(employee => employee.BranchId == branchId)
                    .OrderByDescending(employee => employee.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
