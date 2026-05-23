using salasries7.Models;

namespace salasries7.Services;

public interface IBranchContext
{
    int? BranchId { get; }
    void SetBranch(int branchId);
    void Clear();
    Task<Branch?> GetSelectedBranchAsync();
    Task<IReadOnlyList<Branch>> GetActiveBranchesAsync();
}
