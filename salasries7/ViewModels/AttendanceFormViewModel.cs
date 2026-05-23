using Microsoft.AspNetCore.Mvc.Rendering;
using salasries7.Models;

namespace salasries7.ViewModels;

public class AttendanceFormViewModel
{
    public AttendanceRecord Record { get; set; } = new();
    public string? BranchName { get; set; }
    public IEnumerable<SelectListItem> Employees { get; set; } = [];
}

public class AttendanceIndexViewModel
{
    public string BranchName { get; set; } = string.Empty;
    public int Year { get; set; } = DateTime.Today.Year;
    public int Month { get; set; } = DateTime.Today.Month;
    public IReadOnlyList<AttendanceRecord> Records { get; set; } = [];

    public int PresentCount => Records.Count(record => record.Status == AttendanceStatus.Present);
    public int AbsentCount => Records.Count(record => record.Status == AttendanceStatus.Absent);
    public int VacationCount => Records.Count(record => record.Status is AttendanceStatus.Vacation or AttendanceStatus.SickLeave);
    public decimal OvertimeHours => Records.Sum(record => record.OvertimeHours);
    public int LateMinutes => Records.Sum(record => record.LateMinutes);
}
