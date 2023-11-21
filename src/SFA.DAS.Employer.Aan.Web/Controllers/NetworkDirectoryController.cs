using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Extensions;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.NetworkDirectory;
using SFA.DAS.Aan.SharedUi.Models.NetworkEvents;
using SFA.DAS.Aan.SharedUi.Services;
using SFA.DAS.ApprenticeAan.Web.Models.NetworkDirectory;
using SFA.DAS.ApprenticeAan.Web.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/network-directory", Name = SharedRouteNames.NetworkDirectory)]
public class NetworkDirectoryController : Controller
{
    private readonly IOuterApiClient _outerApiClient;

    public NetworkDirectoryController(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, NetworkDirectoryRequestModel request, CancellationToken cancellationToken)
    {
        var resultMembers = await _outerApiClient.GetMembers(QueryStringParameterBuilder.BuildQueryStringParameters(request), cancellationToken);
        var resultRegions = await _outerApiClient.GetRegions(cancellationToken);

        resultRegions.Regions!.Add(new Region() { Area = "Multi-regional", Id = 0, Ordering = resultRegions.Regions.Count + 1 });

        var model = new NetworkDirectoryViewModel
        {
            TotalCount = resultMembers.TotalCount,
            NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId })
        };

        foreach (var member in resultMembers.Members)
        {
            MembersViewModel vm = member;
            vm.MemberProfileLink = Url.RouteUrl(SharedRouteNames.MemberProfile, new { employerAccountId = employerAccountId, id = member.MemberId })!;
            model.Members.Add(vm);
        }
        var filterUrl = FilterBuilder.BuildFullQueryString(request, () => Url.RouteUrl(SharedRouteNames.NetworkDirectory)!);
        var filterChoices = PopulateFilterChoices(request, resultRegions.Regions);

        model.PaginationViewModel = SetupPagination(resultMembers, filterUrl);
        model.FilterChoices = filterChoices;
        model.SelectedFiltersModel.SelectedFilters = FilterBuilder.Build(request, () => Url.RouteUrl(SharedRouteNames.NetworkDirectory)!, filterChoices.RoleChecklistDetails.Lookups, filterChoices.RegionChecklistDetails.Lookups);
        model.SelectedFiltersModel.ClearSelectedFiltersLink = Url.RouteUrl(SharedRouteNames.NetworkDirectory)!;
        return View(model);
    }

    private static PaginationViewModel SetupPagination(GetMembersResponse result, string filterUrl)
    {
        var pagination = new PaginationViewModel(result.Page, result.PageSize, result.TotalPages, filterUrl);

        return pagination;
    }

    private static DirectoryFilterChoices PopulateFilterChoices(NetworkDirectoryRequestModel request, List<Region> regions)
        => new()
        {
            Keyword = request.Keyword?.Trim(),
            RoleChecklistDetails = new ChecklistDetails
            {
                Title = "Role",
                QueryStringParameterName = "userRole",
                Lookups = new ChecklistLookup[]
                {
                    new(Role.Apprentice.GetDescription(), Role.Apprentice.ToString(), request.UserRole.Exists(x => x == Role.Apprentice)),
                    new(Role.Employer.GetDescription(), Role.Employer.ToString(), request.UserRole.Exists(x => x == Role.Employer)),
                    new(Role.RegionalChair.GetDescription(), Role.RegionalChair.ToString(), request.UserRole.Exists(x => x == Role.RegionalChair))
                }
            },
            RegionChecklistDetails = new ChecklistDetails
            {
                Title = "Region",
                QueryStringParameterName = "regionId",
                Lookups = regions.OrderBy(x => x.Ordering).Select(region => new ChecklistLookup(region.Area, region.Id.ToString(), request.RegionId.Exists(x => x == region.Id))).ToList()
            }
        };
}
