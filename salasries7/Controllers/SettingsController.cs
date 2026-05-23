using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;

using Microsoft.AspNetCore.Authorization;

namespace salasries7.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _db;

    public SettingsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _db.SystemSettings
            .GroupBy(s => s.Category ?? "General")
            .ToListAsync();
        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Update(Dictionary<string, string> settings)
    {
        foreach (var item in settings)
        {
            var dbSetting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == item.Key);
            if (dbSetting != null)
            {
                dbSetting.Value = item.Value;
            }
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = "تم تحديث الإعدادات بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
