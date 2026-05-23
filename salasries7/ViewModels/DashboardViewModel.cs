using salasries7.Models;

namespace salasries7.ViewModels;

public class DashboardViewModel
{
    public Branch? SelectedBranch { get; set; }
    public IReadOnlyList<Branch> AvailableBranches { get; set; } = [];
    public int BranchCount { get; set; }
    public int EmployeeCount { get; set; }
    public int ActiveEmployeeCount { get; set; }
    public int TodayPresentCount { get; set; }
    public int TodayAbsentCount { get; set; }
    public decimal MonthlyGrossPayroll { get; set; }
    public decimal CurrentMonthAdditions { get; set; }
    public decimal CurrentMonthDeductions { get; set; }
    public PayrollRun? LastPayrollRun { get; set; }
    public IReadOnlyList<BranchPayrollSummary> Branches { get; set; } = [];
    public IReadOnlyList<Employee> RecentEmployees { get; set; } = [];

    public bool HasSelectedBranch => SelectedBranch is not null;
}

public class BranchPayrollSummary
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal MonthlyGrossPayroll { get; set; }
}
