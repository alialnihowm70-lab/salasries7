using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Services;

namespace salasries7.Controllers;

public class BranchContextController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBranchContext _branchContext;

    public BranchContextController(ApplicationDbContext db, IBranchContext branchContext)
    {
        _db = db;
        _branchContext = branchContext;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Set(int branchId, string? returnUrl)
    {
        if (branchId <= 0)
        {
            _branchContext.Clear();
        }
        else
        {
            var exists = await _db.Branches.AnyAsync(branch => branch.Id == branchId && branch.IsActive);
            if (exists)
            {
                _branchContext.SetBranch(branchId);
            }
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
