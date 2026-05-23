using Microsoft.AspNetCore.Mvc.Rendering;
using salasries7.Models;

namespace salasries7.ViewModels;

public class EmployeeFormViewModel
{
    public Employee Employee { get; set; } = new();
    public IEnumerable<SelectListItem> Branches { get; set; } = [];
    public string? SelectedBranchName { get; set; }
    public bool IsBranchLocked { get; set; }
}
