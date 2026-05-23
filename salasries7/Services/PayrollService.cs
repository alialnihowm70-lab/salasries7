using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;

namespace salasries7.Services;

public class PayrollService : IPayrollService
{
    private readonly ApplicationDbContext _db;
    private readonly ISettingsService _settings;

    public PayrollService(ApplicationDbContext db, ISettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task<PayrollRun> CreateRunAsync(int year, int month, int branchId)
    {
        var existingRun = await _db.PayrollRuns
            .Include(run => run.Branch)
            .Include(run => run.Lines)
                .ThenInclude(line => line.Employee)
            .FirstOrDefaultAsync(run => run.Year == year && run.Month == month && run.BranchId == branchId);

        if (existingRun is not null)
        {
            return existingRun;
        }

        var employeeQuery = _db.Employees
            .Include(employee => employee.Branch)
            .Where(employee => employee.Status == EmploymentStatus.Active && employee.BranchId == branchId);

        var employees = await employeeQuery
            .OrderBy(employee => employee.EmployeeNumber)
            .ToListAsync();

        var adjustments = await _db.SalaryAdjustments
            .Where(adjustment => adjustment.Month.Year == year && adjustment.Month.Month == month)
            .ToListAsync();

        var activeLoans = await _db.EmployeeLoans
            .Where(l => l.Status == LoanStatus.Active && l.RemainingAmount > 0 && l.StartDate <= new DateTime(year, month, 1))
            .ToListAsync();

        var run = new PayrollRun
        {
            Year = year,
            Month = month,
            BranchId = branchId,
            Status = PayrollRunStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var taxRate = await _settings.GetDecimalSettingAsync("TaxRate", 0.05m);
        var ssRate = await _settings.GetDecimalSettingAsync("SocialSecurityRate", 0.0375m);

        foreach (var employee in employees)
        {
            var empAdjustments = adjustments.Where(adjustment => adjustment.EmployeeId == employee.Id);
            var empLoans = activeLoans.Where(l => l.EmployeeId == employee.Id);
            
            run.Lines.Add(PayrollCalculator.Calculate(employee, empAdjustments, taxRate, ssRate, empLoans));
        }

        _db.PayrollRuns.Add(run);
        await _db.SaveChangesAsync();

        return await _db.PayrollRuns
            .Include(createdRun => createdRun.Branch)
            .Include(createdRun => createdRun.Lines)
                .ThenInclude(line => line.Employee)
            .FirstAsync(createdRun => createdRun.Id == run.Id);
    }

    public async Task<PayrollRun?> RecalculateRunAsync(int runId, int branchId)
    {
        var run = await _db.PayrollRuns
            .Include(item => item.Lines)
            .FirstOrDefaultAsync(item => item.Id == runId && item.BranchId == branchId);

        if (run is null)
        {
            return null;
        }

        if (run.Status != PayrollRunStatus.Draft)
        {
            return await LoadRunAsync(run.Id);
        }

        var employees = await _db.Employees
            .Where(employee => employee.Status == EmploymentStatus.Active && employee.BranchId == branchId)
            .OrderBy(employee => employee.EmployeeNumber)
            .ToListAsync();

        var adjustments = await _db.SalaryAdjustments
            .Where(adjustment => adjustment.Month.Year == run.Year && adjustment.Month.Month == run.Month)
            .ToListAsync();

        var activeLoans = await _db.EmployeeLoans
            .Where(l => l.Status == LoanStatus.Active && l.RemainingAmount > 0 && l.StartDate <= new DateTime(run.Year, run.Month, 1))
            .ToListAsync();

        _db.PayrollLines.RemoveRange(run.Lines);
        await _db.SaveChangesAsync();

        var taxRate = await _settings.GetDecimalSettingAsync("TaxRate", 0.05m);
        var ssRate = await _settings.GetDecimalSettingAsync("SocialSecurityRate", 0.0375m);

        var lines = employees.Select(employee =>
        {
            var empAdjustments = adjustments.Where(adjustment => adjustment.EmployeeId == employee.Id);
            var empLoans = activeLoans.Where(l => l.EmployeeId == employee.Id);

            var line = PayrollCalculator.Calculate(employee, empAdjustments, taxRate, ssRate, empLoans);

            line.PayrollRunId = run.Id;
            return line;
        });

        _db.PayrollLines.AddRange(lines);
        await _db.SaveChangesAsync();

        return await LoadRunAsync(run.Id);
    }

    public async Task FinalizeRunAsync(int runId, int branchId)
    {
        var run = await _db.PayrollRuns
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == runId && r.BranchId == branchId);

        if (run is null || run.Status == PayrollRunStatus.Paid) return;

        var employeeIds = run.Lines.Select(l => l.EmployeeId).ToList();
        var activeLoans = await _db.EmployeeLoans
            .Where(l => employeeIds.Contains(l.EmployeeId) && l.Status == LoanStatus.Active && l.RemainingAmount > 0)
            .ToListAsync();

        foreach (var loan in activeLoans)
        {
            var line = run.Lines.FirstOrDefault(l => l.EmployeeId == loan.EmployeeId);
            if (line == null) continue;

            // Re-calculate the installment that was actually deducted in this run
            var installmentDeducted = Math.Min(loan.MonthlyInstallment, loan.RemainingAmount);
            
            loan.RemainingAmount -= installmentDeducted;
            if (loan.RemainingAmount <= 0)
            {
                loan.RemainingAmount = 0;
                loan.Status = LoanStatus.PaidOff;
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task<PayrollRun> LoadRunAsync(int runId)
    {
        return await _db.PayrollRuns
            .Include(run => run.Branch)
            .Include(run => run.Lines)
                .ThenInclude(line => line.Employee)
            .FirstAsync(run => run.Id == runId);
    }
}
