using salasries7.Models;

namespace salasries7.Services;

public static class PayrollCalculator
{
    public static PayrollLine Calculate(Employee employee, IEnumerable<SalaryAdjustment> adjustments, decimal taxRate, decimal ssRate, IEnumerable<EmployeeLoan>? activeLoans = null)
    {
        var adjustmentList = adjustments.ToList();
        var bonuses = adjustmentList
            .Where(adjustment => adjustment.Type == SalaryAdjustmentType.Bonus)
            .Sum(adjustment => adjustment.Amount);
        var overtime = adjustmentList
            .Where(adjustment => adjustment.Type == SalaryAdjustmentType.Overtime)
            .Sum(adjustment => adjustment.Amount);
        var deductions = adjustmentList
            .Where(adjustment => adjustment.Type is SalaryAdjustmentType.Deduction or SalaryAdjustmentType.Loan)
            .Sum(adjustment => adjustment.Amount);

        // Add automated loan installments
        var loanInstallments = 0m;
        if (activeLoans != null)
        {
            loanInstallments = activeLoans
                .Where(l => l.EmployeeId == employee.Id && l.Status == LoanStatus.Active && l.RemainingAmount > 0)
                .Sum(l => Math.Min(l.MonthlyInstallment, l.RemainingAmount));
        }

        var totalDeductions = deductions + loanInstallments;
        var taxableGross = employee.BasicSalary + employee.TotalAllowances + bonuses + overtime;
        
        // Dynamic calculations based on settings
        var ssDeduction = taxableGross * ssRate;
        var taxDeduction = (taxableGross - ssDeduction) * taxRate;

        var statutoryDeductions = ssDeduction + taxDeduction;

        return new PayrollLine
        {
            EmployeeId = employee.Id,
            BasicSalary = employee.BasicSalary,
            TotalAllowances = employee.TotalAllowances,
            OvertimePay = overtime,
            Bonuses = bonuses,
            GrossSalary = taxableGross,
            SocialSecurityDeduction = ssDeduction,
            TaxDeduction = taxDeduction,
            OtherDeductions = totalDeductions,
            NetSalary = taxableGross - statutoryDeductions - totalDeductions
        };
    }
}
