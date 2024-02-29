using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Configuration;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
public class ContactUsController : Controller
{
    private readonly ApplicationConfiguration _applicationConfiguration;
    public const string ContactUsViewPath = "~/Views/ContactUs/Index.cshtml";

    public ContactUsController(ApplicationConfiguration applicationConfiguration)
    {
        _applicationConfiguration = applicationConfiguration;
    }

    [HttpGet]
    [Route("accounts/{employerAccountId}/contact-us", Name = SharedRouteNames.ContactUs)]
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var contactUsEmails = _applicationConfiguration.ContactUsEmails;

        var viewModel = new ContactUsViewModel
        {
            EastMidlandsEmailAddress = contactUsEmails.EastMidlands,
            EastOfEnglandEmailAddress = contactUsEmails.EastOfEngland,
            LondonEmailAddress = contactUsEmails.London,
            NorthEastEmailAddress = contactUsEmails.NorthEast,
            NorthWestEmailAddress = contactUsEmails.NorthWest,
            SouthEastEmailAddress = contactUsEmails.SouthEast,
            SouthWestEmailAddress = contactUsEmails.SouthWest,
            WestMidlandsEmailAddress = contactUsEmails.WestMidlands,
            YorkshireAndTheHumberEmailAddress = contactUsEmails.YorkshireAndTheHumber,
            NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId })
        };

        return View(ContactUsViewPath, viewModel);
    }
}
