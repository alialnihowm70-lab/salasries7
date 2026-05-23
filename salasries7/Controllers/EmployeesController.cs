using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using salasries7.Services;
using salasries7.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace salasries7.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBranchContext _branchContext;
    private readonly IWebHostEnvironment _host;

    public EmployeesController(ApplicationDbContext db, IBranchContext branchContext, IWebHostEnvironment host)
    {
        _db = db;
        _branchContext = branchContext;
        _host = host;
    }

    public async Task<IActionResult> Index()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var employees = await _db.Employees
            .Include(employee => employee.Branch)
            .Where(employee => employee.BranchId == selectedBranch.Id)
            .OrderBy(employee => employee.EmployeeNumber)
            .ToListAsync();

        return View(employees);
    }

    public async Task<IActionResult> Details(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var employee = await _db.Employees
            .Include(item => item.Branch)
            .Include(item => item.SalaryAdjustments.OrderByDescending(adjustment => adjustment.Month))
            .Include(item => item.AttendanceRecords.OrderByDescending(record => record.WorkDate).Take(15))
            .Include(item => item.Documents.OrderByDescending(doc => doc.UploadDate))
            .Include(item => item.Loans.OrderByDescending(loan => loan.CreatedDate))
            .Include(item => item.Leaves.OrderByDescending(leave => leave.StartDate))
            .FirstOrDefaultAsync(item => item.Id == id && item.BranchId == selectedBranch.Id);

        return employee is null ? NotFound() : View(employee);
    }

    public async Task<IActionResult> Create()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new EmployeeFormViewModel
        {
            Employee = new Employee { BranchId = selectedBranch.Id },
            Branches = GetLockedBranchOptions(selectedBranch),
            SelectedBranchName = selectedBranch.Name,
            IsBranchLocked = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        model.Employee.BranchId = selectedBranch.Id;

        if (await _db.Employees.AnyAsync(item => item.EmployeeNumber == model.Employee.EmployeeNumber))
        {
            ModelState.AddModelError("Employee.EmployeeNumber", "رقم الموظف مستخدم من قبل");
        }

        if (!ModelState.IsValid)
        {
            model.Branches = GetLockedBranchOptions(selectedBranch);
            model.SelectedBranchName = selectedBranch.Name;
            model.IsBranchLocked = true;
            return View(model);
        }

        _db.Employees.Add(model.Employee);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var employee = await _db.Employees
            .FirstOrDefaultAsync(item => item.Id == id && item.BranchId == selectedBranch.Id);

        if (employee is null)
        {
            return NotFound();
        }

        return View(new EmployeeFormViewModel
        {
            Employee = employee,
            Branches = GetLockedBranchOptions(selectedBranch),
            SelectedBranchName = selectedBranch.Name,
            IsBranchLocked = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (id != model.Employee.Id)
        {
            return NotFound();
        }

        var employeeBelongsToBranch = await _db.Employees
            .AnyAsync(item => item.Id == id && item.BranchId == selectedBranch.Id);

        if (!employeeBelongsToBranch)
        {
            return NotFound();
        }

        model.Employee.BranchId = selectedBranch.Id;

        if (await _db.Employees.AnyAsync(item => item.EmployeeNumber == model.Employee.EmployeeNumber && item.Id != model.Employee.Id))
        {
            ModelState.AddModelError("Employee.EmployeeNumber", "رقم الموظف مستخدم من قبل");
        }

        if (!ModelState.IsValid)
        {
            model.Branches = GetLockedBranchOptions(selectedBranch);
            model.SelectedBranchName = selectedBranch.Name;
            model.IsBranchLocked = true;
            return View(model);
        }

        _db.Update(model.Employee);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var employee = await _db.Employees
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id && item.BranchId == selectedBranch.Id);

        return employee is null ? NotFound() : View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var employee = await _db.Employees
            .FirstOrDefaultAsync(item => item.Id == id && item.BranchId == selectedBranch.Id);

        if (employee is null)
        {
            return NotFound();
        }

        var hasLinkedRecords =
            await _db.PayrollLines.AnyAsync(line => line.EmployeeId == id)
            || await _db.AttendanceRecords.AnyAsync(record => record.EmployeeId == id)
            || await _db.SalaryAdjustments.AnyAsync(adjustment => adjustment.EmployeeId == id);

        if (hasLinkedRecords)
        {
            ModelState.AddModelError(string.Empty, "لا يمكن حذف موظف لديه حضور أو حركات مرتب أو تشغيلات سابقة. غيّر حالته إلى منتهي من صفحة التعديل للحفاظ على السجل.");
            employee.Branch = selectedBranch;
            return View(employee);
        }

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(int employeeId, salasries7.Models.DocumentType type, DateTime? expiryDate, string? notes, IFormFile file)
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null) return RedirectToAction("Index", "Home");

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.BranchId == selectedBranch.Id);
        if (employee is null) return NotFound();

        if (file != null && file.Length > 0)
        {
            var uploadsFolder = Path.Combine(_host.WebRootPath, "uploads", "documents", employeeId.ToString());
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var document = new EmployeeDocument
            {
                EmployeeId = employeeId,
                Type = type,
                FileName = file.FileName,
                FilePath = $"/uploads/documents/{employeeId}/{uniqueFileName}",
                ExpiryDate = expiryDate,
                Notes = notes,
                UploadDate = DateTime.Now
            };

            _db.EmployeeDocuments.Add(document);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = employeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var document = await _db.EmployeeDocuments.Include(d => d.Employee).FirstOrDefaultAsync(d => d.Id == id);
        if (document is null) return NotFound();

        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        if (selectedBranch is null || document.Employee?.BranchId != selectedBranch.Id) return Forbid();

        var employeeId = document.EmployeeId;
        
        // Remove physical file
        var fullPath = Path.Combine(_host.WebRootPath, document.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

        _db.EmployeeDocuments.Remove(document);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = employeeId });
    }

    private static IReadOnlyList<SelectListItem> GetLockedBranchOptions(Branch selectedBranch)
    {
        return
        [
            new SelectListItem(selectedBranch.Name, selectedBranch.Id.ToString(), selected: true)
        ];
    }
}
