using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize]
public class LinksController : Controller
{
    [Route("[controller]/{notificationId}")]
    public IActionResult Index(Guid notificationId)
    {
        var notification = GetNotification(notificationId);

        string employerAccountId = GetEncodedAccountId(notification.EmployerAccountId);

        return RedirectToRoute(RouteNames.PublicProfile, new { employerAccountId = employerAccountId, memberId = notification.MemberId });
    }

    private Notification GetNotification(Guid notificationId) => new(Guid.NewGuid(), 15601);

    private string GetEncodedAccountId(long accountId) => "v6gr7d";
}

public record Notification(Guid MemberId, long EmployerAccountId);
