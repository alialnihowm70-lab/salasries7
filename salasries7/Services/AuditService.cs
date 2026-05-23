using salasries7.Data;
using salasries7.Models;

namespace salasries7.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string? entityId, string? changes = null);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityName, string? entityId, string? changes = null)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Changes = changes,
            ModifiedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
            IPAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.Now
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
