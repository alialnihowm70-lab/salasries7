using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using salasries7.Services;

namespace salasries7.Controllers;

public class SyncController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ISyncService _syncService;
    private readonly IConfiguration _config;

    public SyncController(ApplicationDbContext db, ISyncService syncService, IConfiguration config)
    {
        _db = db;
        _syncService = syncService;
        _config = config;
    }

    // --- UI Actions (For Desktop Client) ---

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Dashboard()
    {
        var unsyncedCount = await _db.Employees.CountAsync(e => !e.IsSynced)
                          + await _db.PayrollRuns.CountAsync(r => !r.IsSynced)
                          + await _db.EmployeeLoans.CountAsync(l => !l.IsSynced);

        ViewBag.UnsyncedCount = unsyncedCount;
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RunPush()
    {
        var result = await _syncService.PushAllAsync();
        return Json(result);
    }

    // --- API Endpoints (For Web Portal on Render) ---

    [AllowAnonymous]
    [HttpPost("api/sync/receive-employees")]
    public async Task<IActionResult> ReceiveEmployees([FromBody] List<Employee> employees)
    {
        var apiKey = _config["SyncSettings:ApiKey"];
        if (!Request.Headers.TryGetValue("X-Sync-Key", out var providedKey) || providedKey != apiKey)
        {
            return Unauthorized("Invalid Sync Key");
        }

        foreach (var emp in employees)
        {
            var existing = await _db.Employees.FirstOrDefaultAsync(e => e.SyncId == emp.SyncId);
            if (existing != null)
            {
                // Simple versioning: update if remote is newer
                if (emp.UpdatedAt > existing.UpdatedAt)
                {
                    _db.Entry(existing).CurrentValues.SetValues(emp);
                    existing.IsSynced = true;
                }
            }
            else
            {
                emp.Id = 0; // Ensure new insertion
                emp.IsSynced = true;
                _db.Employees.Add(emp);
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { Count = employees.Count });
    }
}
