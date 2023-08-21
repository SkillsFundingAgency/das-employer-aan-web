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

        var routeName = notification.TemplateName switch
        {
            "Onboarding" => RouteNames.NetworkHub,
            "Sign-Up" => RedirectToRoute(RouteNames.EventDetails, new { }
        };

        return RedirectToRoute(RouteNames.PublicProfile, new { employerAccountId = employerAccountId, memberId = notification.MemberId });
    }

    private Notification GetNotification(Guid notificationId) => new(Guid.NewGuid(), 15601, "Onboarding", "{'parametervalue'=''}");

    private string GetEncodedAccountId(long accountId) => "v6gr7d";
}

public record Notification(Guid MemberId, long EmployerAccountId, string TemplateName, string Tokens);


record Token(string ParameterValue, string Link);


/*
 * rehydration of link logic is in web
 * Job will inject the host in the token (EmpAanBaseUrl, AppAanBaseUrl)
 * 
 * 
 * */

/* Tokens AccountId, EventId, ParameterValue, NotificationId, BaseUrl


/// {{link}}

/// {hostname}/links/{notificationId}




