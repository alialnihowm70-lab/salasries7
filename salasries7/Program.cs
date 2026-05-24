using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using salasries7.Data;
using salasries7.Services;
using System.Globalization;
using ElectronNET.API;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (ElectronNET.API.HybridSupport.IsElectronActive)
    {
        options.UseSqlite("Data Source=salasries7_dev.db");
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Salasries7.Branch";
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<IBranchContext, BranchContext>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISyncService, SyncService>();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddControllersWithViews();

// Add Electron usage
builder.WebHost.UseElectron(args);

var app = builder.Build();

// Electron Window Management
if (ElectronNET.API.HybridSupport.IsElectronActive)
{
    Task.Run(async () => {
        var options = new ElectronNET.API.Entities.BrowserWindowOptions
        {
            Width = 1400,
            Height = 900,
            Show = false,
            Title = "منظومة المرتبات الاحترافية - نسخة سطح المكتب",
            WebPreferences = new ElectronNET.API.Entities.WebPreferences
            {
                NodeIntegration = false
            }
        };
        var window = await ElectronNET.API.Electron.WindowManager.CreateWindowAsync(options);
        window.OnReadyToShow += () => window.Show();
        window.SetTitle("منظومة المرتبات الاحترافية - الإصدار السابع");
    });
}

var arabicCulture = new CultureInfo("ar-LY");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(arabicCulture),
    SupportedCultures = [arabicCulture],
    SupportedUICultures = [arabicCulture]
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Hybrid Schema Initialization (SQLite for local, PostgreSQL for cloud)
    string fixSql;
    if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
    {
        fixSql = @"
            CREATE TABLE IF NOT EXISTS ""SystemSettings"" (""Key"" TEXT PRIMARY KEY, ""Value"" TEXT NOT NULL, ""Description"" TEXT, ""Category"" TEXT);
            CREATE TABLE IF NOT EXISTS ""GlobalNotifications"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""Title"" TEXT NOT NULL, ""Message"" TEXT NOT NULL, ""CreatedAt"" TEXT NOT NULL, ""IsRead"" INTEGER NOT NULL, ""Severity"" INTEGER NOT NULL, ""ActionUrl"" TEXT);
            CREATE TABLE IF NOT EXISTS ""Users"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""Username"" TEXT NOT NULL, ""PasswordHash"" TEXT NOT NULL, ""Role"" INTEGER NOT NULL, ""EmployeeId"" INTEGER, ""CreatedAt"" TEXT NOT NULL, ""IsActive"" INTEGER NOT NULL);
            
            -- Employee Columns
            ALTER TABLE ""Employees"" ADD COLUMN ""PassportNumber"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""PassportExpiry"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""PhoneNumber"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""PersonalEmail"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""NationalId"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""NationalIdExpiry"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""JobTitle"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""Department"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""FullAddress"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""BankName"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""AccountNumber"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""IBAN"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""Notes"" TEXT;

            -- Sync Columns
            ALTER TABLE ""Employees"" ADD COLUMN ""SyncId"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""UpdatedAt"" TEXT;
            ALTER TABLE ""Employees"" ADD COLUMN ""IsSynced"" INTEGER DEFAULT 0;
            -- (Note: SQLite handles 'ADD COLUMN' gracefully if they exist in some versions, but usually we just wrap in try-catch or check)
        ";
    }
    else
    {
        fixSql = @"
            CREATE TABLE IF NOT EXISTS ""SystemSettings"" (""Key"" TEXT PRIMARY KEY, ""Value"" TEXT NOT NULL, ""Description"" TEXT, ""Category"" TEXT);
            CREATE TABLE IF NOT EXISTS ""GlobalNotifications"" (""Id"" SERIAL PRIMARY KEY, ""Title"" TEXT NOT NULL, ""Message"" TEXT NOT NULL, ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL, ""IsRead"" BOOLEAN NOT NULL, ""Severity"" INTEGER NOT NULL, ""ActionUrl"" TEXT);
            CREATE TABLE IF NOT EXISTS ""Users"" (""Id"" SERIAL PRIMARY KEY, ""Username"" TEXT NOT NULL, ""PasswordHash"" TEXT NOT NULL, ""Role"" INTEGER NOT NULL, ""EmployeeId"" INTEGER, ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL, ""IsActive"" BOOLEAN NOT NULL);
            DO $$ 
            BEGIN 
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='SystemSettings' AND column_name='Category') THEN
                    ALTER TABLE ""SystemSettings"" ADD COLUMN ""Category"" TEXT NULL;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Employees' AND column_name='Notes') THEN
                    ALTER TABLE ""Employees"" ADD COLUMN ""PassportNumber"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""PassportExpiry"" TIMESTAMP WITH TIME ZONE NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""PhoneNumber"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""PersonalEmail"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""NationalId"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""NationalIdExpiry"" TIMESTAMP WITH TIME ZONE NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""JobTitle"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""Department"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""FullAddress"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""BankName"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""AccountNumber"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""IBAN"" TEXT NULL;
                    ALTER TABLE ""Employees"" ADD COLUMN ""Notes"" TEXT NULL;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Employees' AND column_name='SyncId') THEN
                    DECLARE t TEXT;
                    BEGIN
                        FOR t IN (SELECT table_name FROM information_schema.tables WHERE table_schema='public') 
                        LOOP
                            EXECUTE format('ALTER TABLE %I ADD COLUMN IF NOT EXISTS ""SyncId"" UUID DEFAULT gen_random_uuid() NOT NULL', t);
                            EXECUTE format('ALTER TABLE %I ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP WITH TIME ZONE DEFAULT NOW() NOT NULL', t);
                            EXECUTE format('ALTER TABLE %I ADD COLUMN IF NOT EXISTS ""IsSynced"" BOOLEAN DEFAULT FALSE NOT NULL', t);
                        END LOOP;
                    END;
                END IF;
            END $$;";
    }

    try { await db.Database.ExecuteSqlRawAsync(fixSql); } catch { /* Ignore idempotent errors */ }

    // Seed default settings if missing
    var defaults = new List<salasries7.Models.SystemSetting>
    {
        new() { Key = "TaxRate", Value = "0.05", Description = "نسبة ضريبة الدخل (مثال: 0.05 لـ 5%)", Category = "Finance" },
        new() { Key = "SocialSecurityRate", Value = "0.0375", Description = "نسبة الضمان الاجتماعي (حصة الموظف)", Category = "Finance" },
        new() { Key = "CompanySocialSecurityRate", Value = "0.105", Description = "نسبة الضمان الاجتماعي (حصة الشركة)", Category = "Finance" },
        new() { Key = "CurrencyCode", Value = "د.ل", Description = "رمز العملة المستخدم في التقارير", Category = "General" },
        new() { Key = "CompanyName", Value = "منظومة المرتبات الاحترافية", Description = "اسم الشركة الذي يظهر في التقارير", Category = "General" }
    };

    foreach (var d in defaults)
    {
        if (!db.SystemSettings.Any(s => s.Key == d.Key))
        {
            db.SystemSettings.Add(d);
        }
    }
    await db.SaveChangesAsync();

    await db.SaveChangesAsync();

    // Seed default admin if missing
    if (!db.Users.Any(u => u.Username == "admin"))
    {
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<salasries7.Models.AppUser>();
        var admin = new salasries7.Models.AppUser
        {
            Username = "admin",
            Role = salasries7.Models.UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        admin.PasswordHash = hasher.HashPassword(admin, "admin123");
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }

    await DatabaseSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
