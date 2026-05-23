using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Models;
using System.Net.Http.Json;

namespace salasries7.Services;

public interface ISyncService
{
    Task<SyncResult> PushAllAsync();
    Task<SyncResult> PullAllAsync();
}

public class SyncResult
{
    public int Pushed { get; set; }
    public int Pulled { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SyncService : ISyncService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _clientFactory;

    public SyncService(ApplicationDbContext db, IConfiguration config, IHttpClientFactory clientFactory)
    {
        _db = db;
        _config = config;
        _clientFactory = clientFactory;
    }

    public async Task<SyncResult> PushAllAsync()
    {
        var result = new SyncResult { Success = true };
        var remoteUrl = _config["SyncSettings:RemoteUrl"];
        var apiKey = _config["SyncSettings:ApiKey"];

        if (string.IsNullOrEmpty(remoteUrl)) return new SyncResult { Success = false, Message = "Remote URL not configured." };

        using var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Sync-Key", apiKey);

        // Batch Push for Employees (Example)
        var unsyncedEmployees = await _db.Employees.Where(e => !e.IsSynced).ToListAsync();
        if (unsyncedEmployees.Any())
        {
            var response = await client.PostAsJsonAsync($"{remoteUrl}/api/sync/receive-employees", unsyncedEmployees);
            if (response.IsSuccessStatusCode)
            {
                foreach (var e in unsyncedEmployees) e.IsSynced = true;
                result.Pushed += unsyncedEmployees.Count;
            }
            else
            {
                result.Success = false;
                result.Message += "Failed to push employees. ";
            }
        }

        // Add more logic for other models (Branches, Payroll, etc.)
        
        await _db.SaveChangesAsync();
        return result;
    }

    public async Task<SyncResult> PullAllAsync()
    {
        // Implementation for pulling data from remote
        return new SyncResult { Success = true, Message = "Pull logic pending." };
    }
}
