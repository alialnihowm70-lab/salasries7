using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;

namespace salasries7.Controllers;

public class BranchesController : Controller
{
    private readonly ApplicationDbContext _db;

    public BranchesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var branches = await _db.Branches
            .Include(branch => branch.Employees)
            .OrderBy(branch => branch.Name)
            .ToListAsync();

        return View(branches);
    }

    public async Task<IActionResult> Details(int id)
    {
        var branch = await _db.Branches
            .Include(item => item.Employees.OrderBy(employee => employee.EmployeeNumber))
            .FirstOrDefaultAsync(item => item.Id == id);

        return branch is null ? NotFound() : View(branch);
    }

    public IActionResult Create()
    {
        return View(new Branch());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Branch branch)
    {
        if (await _db.Branches.AnyAsync(item => item.Code == branch.Code))
        {
            ModelState.AddModelError(nameof(Branch.Code), "رمز الفرع مستخدم من قبل");
        }

        if (!ModelState.IsValid)
        {
            return View(branch);
        }

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await _db.Branches.FindAsync(id);
        return branch is null ? NotFound() : View(branch);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Branch branch)
    {
        if (id != branch.Id)
        {
            return NotFound();
        }

        if (await _db.Branches.AnyAsync(item => item.Code == branch.Code && item.Id != branch.Id))
        {
            ModelState.AddModelError(nameof(Branch.Code), "رمز الفرع مستخدم من قبل");
        }

        if (!ModelState.IsValid)
        {
            return View(branch);
        }

        _db.Update(branch);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var branch = await _db.Branches
            .Include(item => item.Employees)
            .FirstOrDefaultAsync(item => item.Id == id);

        return branch is null ? NotFound() : View(branch);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var branch = await _db.Branches
            .Include(item => item.Employees)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (branch is null)
        {
            return NotFound();
        }

        if (branch.Employees.Any())
        {
            ModelState.AddModelError(string.Empty, "لا يمكن حذف فرع مرتبط بموظفين. عطله بدل الحذف أو انقل الموظفين لفرع آخر.");
            return View(branch);
        }

        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
