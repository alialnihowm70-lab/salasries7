using Microsoft.EntityFrameworkCore;
using salasries7.Models;
using salasries7.Services;

namespace salasries7.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Branches.AnyAsync())
        {
            return;
        }

        var tripoli = new Branch
        {
            Code = "TRP",
            Name = "الإدارة الرئيسية",
            City = "طرابلس",
            Address = "شارع عمر المختار"
        };

        var benghazi = new Branch
        {
            Code = "BEN",
            Name = "فرع بنغازي",
            City = "بنغازي",
            Address = "منطقة الفويهات"
        };

        db.Branches.AddRange(tripoli, benghazi);

        var employees = new List<Employee>
        {
            new()
            {
                EmployeeNumber = "EMP-001",
                FullName = "أحمد محمد سالم",
                NationalId = "119900001234",
                JobTitle = "مدير مالي",
                Department = "الإدارة المالية",
                Branch = tripoli,
                HireDate = new DateTime(2022, 1, 15),
                BasicSalary = 1800,
                HousingAllowance = 350,
                TransportationAllowance = 120,
                OtherAllowance = 80,
                SocialSecurityDeduction = 90,
                TaxDeduction = 45
            },
            new()
            {
                EmployeeNumber = "EMP-002",
                FullName = "سارة علي منصور",
                NationalId = "219940004321",
                JobTitle = "أخصائية موارد بشرية",
                Department = "الموارد البشرية",
                Branch = tripoli,
                HireDate = new DateTime(2023, 3, 1),
                BasicSalary = 1250,
                HousingAllowance = 250,
                TransportationAllowance = 100,
                SocialSecurityDeduction = 62.5m,
                TaxDeduction = 20
            },
            new()
            {
                EmployeeNumber = "EMP-003",
                FullName = "خالد نوري الفيتوري",
                NationalId = "119880009999",
                JobTitle = "مسؤول تشغيل",
                Department = "التشغيل",
                Branch = benghazi,
                HireDate = new DateTime(2021, 8, 20),
                BasicSalary = 1450,
                HousingAllowance = 280,
                TransportationAllowance = 110,
                OtherAllowance = 60,
                SocialSecurityDeduction = 72.5m,
                TaxDeduction = 25
            }
        };

        db.Employees.AddRange(employees);
        await db.SaveChangesAsync();

        var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        db.SalaryAdjustments.AddRange(
            new SalaryAdjustment
            {
                EmployeeId = employees[0].Id,
                Month = month,
                Type = SalaryAdjustmentType.Bonus,
                Title = "مكافأة إنجاز شهري",
                Amount = 150
            },
            new SalaryAdjustment
            {
                EmployeeId = employees[1].Id,
                Month = month,
                Type = SalaryAdjustmentType.Deduction,
                Title = "خصم تأخير",
                Amount = 25
            },
            new SalaryAdjustment
            {
                EmployeeId = employees[2].Id,
                Month = month,
                Type = SalaryAdjustmentType.Overtime,
                Title = "ساعات إضافية",
                Amount = 95
            });

        await db.SaveChangesAsync();

        var adjustments = await db.SalaryAdjustments
            .Where(adjustment => adjustment.Month.Year == month.Year && adjustment.Month.Month == month.Month)
            .ToListAsync();

        foreach (var branch in new[] { tripoli, benghazi })
        {
            var run = new PayrollRun
            {
                Year = month.Year,
                Month = month.Month,
                BranchId = branch.Id,
                Status = PayrollRunStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var employee in employees.Where(employee => employee.BranchId == branch.Id))
            {
                run.Lines.Add(PayrollCalculator.Calculate(
                    employee, 
                    adjustments.Where(adjustment => adjustment.EmployeeId == employee.Id),
                    0.05m, // Tax rate
                    0.01m, // Social Security rate
                    new List<EmployeeLoan>()
                ));
            }

            db.PayrollRuns.Add(run);
        }
        await db.SaveChangesAsync();
    }
}
