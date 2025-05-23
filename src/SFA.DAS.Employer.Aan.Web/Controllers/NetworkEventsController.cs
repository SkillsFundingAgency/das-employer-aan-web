﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aan.SharedUi.Constants;
using SFA.DAS.Aan.SharedUi.Extensions;
using SFA.DAS.Aan.SharedUi.Infrastructure;
using SFA.DAS.Aan.SharedUi.Models;
using SFA.DAS.Aan.SharedUi.Models.NetworkEvents;
using SFA.DAS.Aan.SharedUi.Services;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
using SFA.DAS.Employer.Aan.Web.Authentication;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;

namespace SFA.DAS.Employer.Aan.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/network-events", Name = RouteNames.NetworkEvents)]
public class NetworkEventsController : Controller
{
    private readonly IOuterApiClient _outerApiClient;
    private readonly ISessionService _sessionService;

    public NetworkEventsController(IOuterApiClient outerApiClient, ISessionService sessionService)
    {
        _outerApiClient = outerApiClient;
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, GetNetworkEventsRequest request, CancellationToken cancellationToken)
    {
        var calendarEvents = await _outerApiClient.GetCalendarEvents(_sessionService.GetMemberId(), QueryStringParameterBuilder.BuildQueryStringParameters(request), cancellationToken);

        var calendars = calendarEvents.Calendars;
        var regions = calendarEvents.Regions;

        var regionOrdering = (regions.Any()) ? regions.Select(region => region.Ordering).Max() + 1 : 1;

        regions.Add(new Region { Area = "National", Id = 0, Ordering = regionOrdering });

        var model = InitialiseViewModel(employerAccountId,calendarEvents);
        var filterUrl = FilterBuilder.BuildFullQueryString(request, () => Url.RouteUrl(SharedRouteNames.NetworkEvents)!);
        model.PaginationViewModel = SetupPagination(calendarEvents, filterUrl);

        var filterChoices = PopulateFilterChoices(request, calendars, regions);
        model.FilterChoices = filterChoices;
        model.OrderBy = request.OrderBy;
        model.IsInvalidLocation = calendarEvents.IsInvalidLocation;
        model.SearchedLocation = (request.Location != null) ? request.Location : string.Empty;
        model.SelectedFiltersModel.SelectedFilters = FilterBuilder.Build(request, () => Url.RouteUrl(SharedRouteNames.NetworkEvents)!, filterChoices.EventFormatChecklistDetails.Lookups, filterChoices.EventTypeChecklistDetails.Lookups, filterChoices.RegionChecklistDetails.Lookups);
        model.SelectedFiltersModel.ClearSelectedFiltersLink = Url.RouteUrl(SharedRouteNames.NetworkEvents)!;
        return View(model);
    }

    private NetworkEventsViewModel InitialiseViewModel(string employerAccountId, GetCalendarEventsQueryResult result)
    {
        var model = new NetworkEventsViewModel
        {
            TotalCount = result.TotalCount,
            NetworkHubLink = Url.RouteUrl(RouteNames.NetworkHub, new { employerAccountId = employerAccountId })
        };

        foreach (var calendarEvent in result.CalendarEvents)
        {
            CalendarEventViewModel vm = calendarEvent;
            vm.CalendarEventLink = Url.RouteUrl(@RouteNames.NetworkEventDetails, new { employerAccountId = employerAccountId, id = calendarEvent.CalendarEventId })!;
            model.CalendarEvents.Add(vm);
        }
        return model;
    }

    private static PaginationViewModel SetupPagination(GetCalendarEventsQueryResult result, string filterUrl)
    {
        var pagination = new PaginationViewModel(result.Page, result.PageSize, result.TotalPages, filterUrl);

        return pagination;

    }
    private static EventFilterChoices PopulateFilterChoices(GetNetworkEventsRequest request, List<Calendar> calendars, List<Region> regions)
        => new()
        {
            Keyword = request.Keyword?.Trim(),
            Location = request.Location ?? "",
            Radius = request.Radius ?? 5,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            EventFormatChecklistDetails = new ChecklistDetails
            {
                Title = "Event formats",
                QueryStringParameterName = "eventFormat",
                Lookups = new ChecklistLookup[]
                {
                    new(EventFormat.InPerson.GetDescription()!, EventFormat.InPerson.ToString(), request.EventFormat.Exists(x => x == EventFormat.InPerson)),
                    new(EventFormat.Online.GetDescription()!, EventFormat.Online.ToString(), request.EventFormat.Exists(x => x == EventFormat.Online)),
                    new(EventFormat.Hybrid.GetDescription()!, EventFormat.Hybrid.ToString(), request.EventFormat.Exists(x => x == EventFormat.Hybrid))
                }
            },
            EventTypeChecklistDetails = new ChecklistDetails
            {
                Title = "Event types",
                QueryStringParameterName = "calendarId",
                Lookups = calendars.OrderBy(x => x.Ordering).Select(cal => new ChecklistLookup(cal.CalendarName, cal.Id.ToString(), request.CalendarId.Exists(x => x == cal.Id))).ToList(),
            },
            RegionChecklistDetails = new ChecklistDetails
            {
                Title = "Regions",
                QueryStringParameterName = "regionId",
                Lookups = regions.OrderBy(x => x.Ordering).Select(region => new ChecklistLookup(region.Area, region.Id.ToString(), request.RegionId.Exists(x => x == region.Id))).ToList()
            }
        };
}
