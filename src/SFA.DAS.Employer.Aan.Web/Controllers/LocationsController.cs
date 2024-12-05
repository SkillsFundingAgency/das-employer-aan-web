using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Authentication;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
public class LocationsController : Controller
{
    private readonly IOuterApiClient _outerApiClient;

    public LocationsController(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    [HttpGet]
    [Route("/locations/search")]
    public async Task<IActionResult> GetLocationsBySearch([FromQuery] string query, CancellationToken cancellationToken)
    {
        var result = await _outerApiClient.GetLocationsBySearch(query, cancellationToken);
        return Ok(result.Locations);
    }
}