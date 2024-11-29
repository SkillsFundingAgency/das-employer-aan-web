using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Encoding;

namespace SFA.DAS.Employer.Aan.Web.Controllers.Onboarding
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/onboarding/confirm-details", Name = RouteNames.Onboarding.ConfirmDetails)]
    public class ConfirmDetailsController(IEncodingService encodingService, IOuterApiClient apiClient) : Controller
    {
        public const string ViewPath = "~/Views/Onboarding/ConfirmDetails.cshtml";

        [HttpGet]
        public async Task<IActionResult> Index([FromRoute] string employerAccountId)
        {
            var viewModel = await GetViewModel(employerAccountId);
            return View(ViewPath, viewModel);
        }

        [HttpPost]
        public IActionResult Index(ConfirmDetailsSubmitModel submitModel)
        {
            return new RedirectToRouteResult(RouteNames.Onboarding.JoinTheNetwork, new { submitModel.EmployerAccountId });
        }

        private async Task<ConfirmDetailsViewModel> GetViewModel(string employerAccountId)
        {
            var account = User.GetEmployerAccount(employerAccountId);
            var accountId  = encodingService.Decode(employerAccountId.ToUpper(), EncodingType.AccountId);

            var apiResponse = await apiClient.GetOnboardingConfirmDetails(accountId);

            return new ConfirmDetailsViewModel
            {
                BackLink = "",
                EmployerAccountId = employerAccountId,
                ActiveApprenticesCount = apiResponse.NumberOfActiveApprentices,
                Sectors = apiResponse.Sectors,
                FullName = User.GetUserDisplayName(),
                EmailAddress = User.GetEmail(),
                EmployerName = account.EmployerName,
            };
        }
    }
}
