using Microsoft.EntityFrameworkCore;
using salasries7.Data;

namespace salasries7.Services;

public interface ISettingsService
{
    Task<string> GetSettingAsync(string key, string defaultValue = "");
    Task<decimal> GetDecimalSettingAsync(string key, decimal defaultValue = 0);
}

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _db;

    public SettingsService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetSettingAsync(string key, string defaultValue = "")
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value ?? defaultValue;
    }

    public async Task<decimal> GetDecimalSettingAsync(string key, decimal defaultValue = 0)
    {
        var value = await GetSettingAsync(key);
        if (decimal.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }
}
