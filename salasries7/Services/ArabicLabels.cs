using salasries7.Models;

namespace salasries7.Services;

public static class ArabicLabels
{
    public static string For(EmploymentStatus status) => status switch
    {
        EmploymentStatus.Active => "نشط",
        EmploymentStatus.Suspended => "موقوف",
        EmploymentStatus.Ended => "منتهي",
        _ => status.ToString()
    };

    public static string For(PayrollRunStatus status) => status switch
    {
        PayrollRunStatus.Draft => "مسودة",
        PayrollRunStatus.Approved => "معتمد",
        PayrollRunStatus.Paid => "مدفوع",
        _ => status.ToString()
    };

    public static string For(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Present => "حاضر",
        AttendanceStatus.Absent => "غائب",
        AttendanceStatus.Vacation => "إجازة",
        AttendanceStatus.SickLeave => "إجازة مرضية",
        _ => status.ToString()
    };

    public static string For(SalaryAdjustmentType type) => type switch
    {
        SalaryAdjustmentType.Bonus => "مكافأة",
        SalaryAdjustmentType.Deduction => "خصم",
        SalaryAdjustmentType.Loan => "سلفة",
        SalaryAdjustmentType.Overtime => "إضافي",
        _ => type.ToString()
    };

    public static string For(DocumentType type) => type switch
    {
        DocumentType.Passport => "جواز سفر",
        DocumentType.NationalId => "بطاقة شخصية",
        DocumentType.Contract => "عقد عمل",
        DocumentType.Certificate => "شهادة",
        DocumentType.HealthCard => "شهادة صحية",
        DocumentType.Other => "أخرى",
        _ => type.ToString()
    };

    public static string For(LoanStatus status) => status switch
    {
        LoanStatus.Active => "قائم",
        LoanStatus.PaidOff => "مسدد بالكامل",
        LoanStatus.Suspended => "موقوف",
        _ => status.ToString()
    };

    public static string For(LeaveType type) => type switch
    {
        LeaveType.Annual => "سنوية",
        LeaveType.Sick => "مرضية",
        LeaveType.Emergency => "طارئة",
        LeaveType.Unpaid => "بدون مرتب",
        LeaveType.Maternity => "وضع",
        _ => type.ToString()
    };

    public static string MonthName(int month) => month switch
    {
        1 => "يناير",
        2 => "فبراير",
        3 => "مارس",
        4 => "أبريل",
        5 => "مايو",
        6 => "يونيو",
        7 => "يوليو",
        8 => "أغسطس",
        9 => "سبتمبر",
        10 => "أكتوبر",
        11 => "نوفمبر",
        12 => "ديسمبر",
        _ => month.ToString()
    };
}
