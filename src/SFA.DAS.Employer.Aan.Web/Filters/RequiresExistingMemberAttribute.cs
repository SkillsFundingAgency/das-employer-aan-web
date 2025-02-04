using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Extensions;

namespace SFA.DAS.Employer.Aan.Web.Filters;

[ExcludeFromCodeCoverage]
public class RequiresExistingMemberAttribute : ApplicationFilterAttribute
{
    private readonly ISessionService _sessionService;
    private readonly IOuterApiClient _outerApiClient;

    public RequiresExistingMemberAttribute(ISessionService sessionService, IOuterApiClient outerApiClient)
    {
        _sessionService = sessionService;
        _outerApiClient = outerApiClient;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor) return;

        var isValidRequest = await IsValidRequest(context, controllerActionDescriptor);
        if (!isValidRequest)
        {
            context.RouteData.Values.TryGetValue("employerAccountId", out object? accountId);
            context.Result = RedirectToHome(new { EmployerAccountId = accountId });
            return;
        }

        await next();
    }

    private bool BypassCheck(ControllerActionDescriptor controllerActionDescriptor)
    {
        var controllersToByPass = new[]
        {
            nameof(HomeController),
            nameof(ServiceController),
            nameof(AccessDeniedController),
            nameof(LeavingTheNetworkConfirmationController),
            nameof(RejoinTheNetworkController),
            nameof(LocationsController)
        };

        return controllersToByPass.Contains(controllerActionDescriptor.ControllerTypeInfo.Name);
    }

    private async Task<bool> IsValidRequest(ActionExecutingContext context, ControllerActionDescriptor controllerActionDescriptor)
    {
        var userId = context.HttpContext.User.GetUserId();
        if (userId == Guid.Empty) return true;

        var sessionValue = _sessionService.Get(Constants.SessionKeys.MemberId);
        var isLive = _sessionService.GetMemberStatus() == MemberStatus.Live;

        if (sessionValue == null)
        {
            var response = await _outerApiClient.GetEmployerMember(userId, CancellationToken.None);

            var employer = response.GetContent();
            sessionValue = response.ResponseMessage.IsSuccessStatusCode ? employer.MemberId.ToString() : Guid.Empty.ToString();
            _sessionService.Set(Constants.SessionKeys.MemberId, sessionValue.ToUpper());
            var memberStatus = employer.Status;
            _sessionService.Set(Constants.SessionKeys.MemberStatus, memberStatus);
            isLive = memberStatus == MemberStatus.Live.ToString();
        }

        if (BypassCheck(controllerActionDescriptor)) return true;

        var isMember = Guid.Parse(sessionValue) != Guid.Empty;

        var isRequestingOnboardingPage = IsRequestForOnboardingAction(controllerActionDescriptor);

        return (isMember && isLive && !isRequestingOnboardingPage)
               || (!isMember && isRequestingOnboardingPage);
    }
}
