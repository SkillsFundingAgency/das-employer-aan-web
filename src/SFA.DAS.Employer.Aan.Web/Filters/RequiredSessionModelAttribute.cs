using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Filters;

[ExcludeFromCodeCoverage]
public class RequiredSessionModelAttribute : ActionFilterAttribute
{
    public Type ModelType { get; set; } = typeof(OnboardingSessionModel);

    public string ActionName { get; set; } = "Index";

    public string ControllerName { get; set; } = "Home";

    private static readonly string[] controllersToByPass = new[] { nameof(BeforeYouStartController), nameof(TermsAndConditionsController) };

    public string[] ControllersToByPass { get => controllersToByPass; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor) return;

        if (!controllerActionDescriptor.ControllerTypeInfo.FullName!.Contains("Onboarding")) return;

        if (ControllersToByPass.Contains(controllerActionDescriptor.ControllerTypeInfo.Name)) return;

        var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();

        var sessionModel = sessionService!.Get(ModelType.Name);

        if (sessionModel == null)
        {
            context.Result = new RedirectToActionResult(ActionName, ControllerName, null);
        }
    }
}
