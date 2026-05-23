using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;

namespace salasries7.Services;

public class BranchContext : IBranchContext
{
    private const string SelectedBranchKey = "SelectedBranchId";
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchContext(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public int? BranchId => _httpContextAccessor.HttpContext?.Session.GetInt32(SelectedBranchKey);

    public void SetBranch(int branchId)
    {
        _httpContextAccessor.HttpContext?.Session.SetInt32(SelectedBranchKey, branchId);
    }

    public void Clear()
    {
        _httpContextAccessor.HttpContext?.Session.Remove(SelectedBranchKey);
    }

    public async Task<Branch?> GetSelectedBranchAsync()
    {
        if (BranchId is not { } branchId)
        {
            return null;
        }

        var branch = await _db.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == branchId && item.IsActive);

        if (branch is null)
        {
            Clear();
        }

        return branch;
    }

    public async Task<IReadOnlyList<Branch>> GetActiveBranchesAsync()
    {
        return await _db.Branches
            .AsNoTracking()
            .Where(branch => branch.IsActive)
            .OrderBy(branch => branch.Name)
            .ToListAsync();
    }
}
