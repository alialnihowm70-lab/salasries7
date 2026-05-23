using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;

namespace salasries7.Services;

public interface INotificationService
{
    Task<List<GlobalNotification>> GetLatestNotificationsAsync(int limit = 5);
    Task MarkAsReadAsync(int id);
    Task ClearOldNotificationsAsync(int days = 30);
    Task CheckForExpiriesAsync();
    Task CreateNotificationAsync(string title, string message, NotificationSeverity severity, string? actionUrl = null);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<GlobalNotification>> GetLatestNotificationsAsync(int limit = 5)
    {
        return await _db.GlobalNotifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int id)
    {
        var notif = await _db.GlobalNotifications.FindAsync(id);
        if (notif != null)
        {
            notif.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearOldNotificationsAsync(int days = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var old = await _db.GlobalNotifications.Where(n => n.CreatedAt < cutoff).ToListAsync();
        _db.GlobalNotifications.RemoveRange(old);
        await _db.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(string title, string message, NotificationSeverity severity, string? actionUrl = null)
    {
        _db.GlobalNotifications.Add(new GlobalNotification
        {
            Title = title,
            Message = message,
            Severity = severity,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task CheckForExpiriesAsync()
    {
        var threshold = DateTime.Today.AddDays(30);
        
        // 1. Passports
        var expiringPassports = await _db.Employees
            .Where(e => e.PassportExpiry <= threshold && e.Status == EmploymentStatus.Active)
            .ToListAsync();

        foreach (var emp in expiringPassports)
        {
            var title = "انتهاء صلاحية جواز سفر";
            var msg = $"جواز سفر الموظف {emp.FullName} سينتهي بتاريخ {emp.PassportExpiry?.ToShortDateString()}";
            
            if (!await _db.GlobalNotifications.AnyAsync(n => n.Title == title && n.Message == msg && !n.IsRead))
            {
                await CreateNotificationAsync(title, msg, NotificationSeverity.Warning, $"/Employees/Details/{emp.Id}");
            }
        }

        // 2. National IDs
        var expiringIds = await _db.Employees
            .Where(e => e.NationalIdExpiry <= threshold && e.Status == EmploymentStatus.Active)
            .ToListAsync();

        foreach (var emp in expiringIds)
        {
            var title = "انتهاء صلاحية الرقم الوطني";
            var msg = $"الرقم الوطني للموظف {emp.FullName} سينتهي بتاريخ {emp.NationalIdExpiry?.ToShortDateString()}";
            
            if (!await _db.GlobalNotifications.AnyAsync(n => n.Title == title && n.Message == msg && !n.IsRead))
            {
                await CreateNotificationAsync(title, msg, NotificationSeverity.Warning, $"/Employees/Details/{emp.Id}");
            }
        }
    }
}
