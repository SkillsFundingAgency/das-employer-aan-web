using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/onboarding/receive-notifications", Name = RouteNames.Onboarding.ReceiveNotifications)]

    public class ReceiveNotificationsController(IValidator<ReceiveNotificationsSubmitModel> validator)
        : Controller
    {
        public const string ViewPath = "~/Views/Onboarding/ReceiveNotifications.cshtml";
        private readonly IValidator<ReceiveNotificationsSubmitModel> _validator = validator;

        [HttpGet]
        public IActionResult Get([FromRoute] string employerAccountId)
        {
            var viewModel = new ReceiveNotificationsViewModel();
            return View(ViewPath, viewModel);
        }


        [HttpPost]
        public IActionResult Post(ReceiveNotificationsSubmitModel submitModel, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(submitModel);

            if (!result.IsValid)
            {
                var model = new ReceiveNotificationsViewModel { EmployerAccountId = submitModel.EmployerAccountId };
                model.EmployerAccountId = submitModel.EmployerAccountId;
                result.AddToModelState(ModelState);
                return View(ViewPath, model);
            }

            return RedirectToRoute(RouteNames.Onboarding.CheckYourAnswers, new { submitModel.EmployerAccountId });
        }
    }
}
