using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.Employer.Aan.Web.Filters;

[ExcludeFromCodeCoverage]
public abstract class ApplicationFilterAttribute : ActionFilterAttribute
{
    const string DefaultActionName = "Index";
    const string DefaultControllerName = "Home";
    const string OnboardingFilter = "Onboarding";

    protected static RedirectToActionResult RedirectToHome(object? routeValues) => new(DefaultActionName, DefaultControllerName, routeValues);

    protected static bool IsRequestForOnboardingAction(ControllerActionDescriptor action) => action.ControllerTypeInfo.FullName!.Contains(OnboardingFilter);
}
