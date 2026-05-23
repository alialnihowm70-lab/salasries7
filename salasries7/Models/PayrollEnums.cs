namespace salasries7.Models;

public enum EmploymentStatus
{
    Active = 1,
    Suspended = 2,
    Ended = 3
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Vacation = 3,
    SickLeave = 4
}

public enum SalaryAdjustmentType
{
    Bonus = 1,
    Deduction = 2,
    Loan = 3,
    Overtime = 4
}

public enum PayrollRunStatus
{
    Draft = 1,
    Approved = 2,
    Paid = 3
}

public enum DocumentType
{
    Passport = 1,
    NationalId = 2,
    Contract = 3,
    Certificate = 4,
    HealthCard = 5,
    Other = 99
}

public enum LeaveType
{
    Annual = 1,
    Sick = 2,
    Emergency = 3,
    Unpaid = 4,
    Maternity = 5
}

public enum LoanStatus
{
    Active = 1,
    PaidOff = 2,
    Suspended = 3
}

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}
