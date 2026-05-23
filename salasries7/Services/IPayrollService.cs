using salasries7.Models;

namespace salasries7.Services;

public interface IPayrollService
{
    Task<PayrollRun> CreateRunAsync(int year, int month, int branchId);
    Task<PayrollRun?> RecalculateRunAsync(int runId, int branchId);
    Task FinalizeRunAsync(int runId, int branchId);
}
