using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Filters;

[ExcludeFromCodeCoverage]
public class RequiredSessionModelAttribute : ApplicationFilterAttribute
{
    const string DefaultActionName = "Index";
    const string DefaultControllerName = "Home";
    const string OnboardingFilter = "Onboarding";

    private static readonly string[] controllersToByPass = new[] { nameof(BeforeYouStartController), nameof(TermsAndConditionsController) };

    public string[] ControllersToByPass { get => controllersToByPass; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor) return;

        if (BypassCheck(controllerActionDescriptor)) return;

        if (!HasValidSessionModel(context.HttpContext.RequestServices))
        {
            context.Result = RedirectToHome(context.RouteData.Values);
        }
    }

    private static bool BypassCheck(ControllerActionDescriptor controllerActionDescriptor)
    {
        if (!IsRequestForOnboardingAction(controllerActionDescriptor)) return true;

        if (controllersToByPass.Any(c => c == controllerActionDescriptor.ControllerTypeInfo.Name)) return true;

        return false;
    }

    private static bool HasValidSessionModel(IServiceProvider services)
    {
        ISessionService sessionService = services.GetService<ISessionService>()!;

        OnboardingSessionModel sessionModel = sessionService.Get<OnboardingSessionModel>();

        return sessionModel != null && sessionModel.IsValid;
    }
}
