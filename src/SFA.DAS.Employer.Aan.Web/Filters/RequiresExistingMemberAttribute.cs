using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers;
using SFA.DAS.Employer.Aan.Web.Extensions;

namespace SFA.DAS.Employer.Aan.Web.Filters;

[ExcludeFromCodeCoverage]
public class RequiresExistingMemberAttribute : IAsyncActionFilter
{
    
    const string DefaultActionName = "Index";
    const string DefaultControllerName = "Home";
    const string OnboardingFilter = "Onboarding";
    
    private readonly IOuterApiClient _outerApiClient;

    public RequiresExistingMemberAttribute(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }
    
    private bool BypassCheck(ControllerActionDescriptor controllerActionDescriptor)
    {
        var controllersToByPass = new[] { nameof(HomeController), nameof(ServiceController), nameof(AccessDeniedController) };

        return controllersToByPass.Contains(controllerActionDescriptor.ControllerTypeInfo.Name);
    }

    private async Task<bool> IsValidRequest(ActionExecutingContext context, ControllerActionDescriptor controllerActionDescriptor)
    {
        if (IsRequestForOnboardingAction(controllerActionDescriptor))
        {
            return true;
        }
        
        var isMember = await _outerApiClient.GetEmployerMember(context.HttpContext.User.GetIdamsUserId(), CancellationToken.None) ;// context.HttpContext.User.GetAanMemberId() != Guid.Empty;

        return isMember.ResponseMessage.IsSuccessStatusCode;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor) return;

        if (BypassCheck(controllerActionDescriptor))
        {
            await next();
            return;
        }

        if (!(await IsValidRequest(context, controllerActionDescriptor)))
        {
            context.RouteData.Values.TryGetValue("employerAccountId", out object? accountId);
            context.Result = RedirectToHome(new { EmployerAccountId = accountId });
        }
        await next();
    }
    

    private static RedirectToActionResult RedirectToHome(object? routeValues) => new(DefaultActionName, DefaultControllerName, routeValues);

    private static bool IsRequestForOnboardingAction(ControllerActionDescriptor action) => action.ControllerTypeInfo.FullName!.Contains(OnboardingFilter);
}
