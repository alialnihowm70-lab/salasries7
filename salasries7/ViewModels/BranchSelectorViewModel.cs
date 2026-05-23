using salasries7.Models;

namespace salasries7.ViewModels;

public class BranchSelectorViewModel
{
    public int? SelectedBranchId { get; set; }
    public string? SelectedBranchName { get; set; }
    public IReadOnlyList<Branch> Branches { get; set; } = [];
}
