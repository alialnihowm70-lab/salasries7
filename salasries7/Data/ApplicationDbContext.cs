using Microsoft.EntityFrameworkCore;
using salasries7.Models;

namespace salasries7.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<SalaryAdjustment> SalaryAdjustments => Set<SalaryAdjustment>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollLine> PayrollLines => Set<PayrollLine>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<EmployeeLoan> EmployeeLoans { get; set; }
    public DbSet<EmployeeLeave> EmployeeLeaves { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<GlobalNotification> GlobalNotifications { get; set; }
    public DbSet<AppUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Branch>()
            .HasIndex(branch => branch.Code)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(employee => employee.EmployeeNumber)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .Property(employee => employee.Status)
            .HasConversion<string>();

        modelBuilder.Entity<AttendanceRecord>()
            .Property(record => record.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SalaryAdjustment>()
            .Property(adjustment => adjustment.Type)
            .HasConversion<string>();

        modelBuilder.Entity<PayrollRun>()
            .Property(run => run.Status)
            .HasConversion<string>();

        modelBuilder.Entity<PayrollLine>()
            .HasOne(line => line.PayrollRun)
            .WithMany(run => run.Lines)
            .HasForeignKey(line => line.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeDocument>()
            .Property(doc => doc.Type)
            .HasConversion<string>();

        modelBuilder.Entity<EmployeeLoan>()
            .Property(loan => loan.Status)
            .HasConversion<string>();

        modelBuilder.Entity<EmployeeLeave>()
            .Property(e => e.Type)
            .HasConversion<string>();
    }
}
