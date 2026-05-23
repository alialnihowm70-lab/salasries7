using Microsoft.AspNetCore.Mvc;
using salasries7.Services;
using salasries7.ViewModels;

namespace salasries7.ViewComponents;

public class BranchSelectorViewComponent : ViewComponent
{
    private readonly IBranchContext _branchContext;

    public BranchSelectorViewComponent(IBranchContext branchContext)
    {
        _branchContext = branchContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var selectedBranch = await _branchContext.GetSelectedBranchAsync();
        var branches = await _branchContext.GetActiveBranchesAsync();

        return View(new BranchSelectorViewModel
        {
            SelectedBranchId = selectedBranch?.Id,
            SelectedBranchName = selectedBranch?.Name,
            Branches = branches
        });
    }
}
