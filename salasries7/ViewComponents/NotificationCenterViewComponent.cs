using Microsoft.AspNetCore.Mvc;
using salasries7.Services;

namespace salasries7.ViewComponents;

public class NotificationCenterViewComponent : ViewComponent
{
    private readonly INotificationService _notifications;

    public NotificationCenterViewComponent(INotificationService notifications)
    {
        _notifications = notifications;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Run expiry check whenever the bell is rendered (can be optimized later)
        await _notifications.CheckForExpiriesAsync();
        
        var list = await _notifications.GetLatestNotificationsAsync(10);
        return View(list);
    }
}
