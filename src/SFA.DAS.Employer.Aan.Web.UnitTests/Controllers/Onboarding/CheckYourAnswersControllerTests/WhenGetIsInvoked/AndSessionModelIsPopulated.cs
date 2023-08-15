using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Constants;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Extensions;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Models;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.CheckYourAnswersControllerTests.WhenGetIsInvoked;

public class AndSessionModelIsPopulated
{
    Mock<IOuterApiClient> _outerApiClientMock;
    Mock<IValidator<CheckYourAnswersSubmitModel>> _validatorMock;
    OnboardingSessionModel _sessionModel;
    CheckYourAnswersController _sut;
    ViewResult? _getResult;
    CheckYourAnswersViewModel? _viewModel;
    Mock<ISessionService> _sessionServiceMock;
    string _employerAccountId;
    ClaimsPrincipal user;

    static readonly string RegionUrl = Guid.NewGuid().ToString();
    static readonly string ReasonToJoinTheNetworkUrl = Guid.NewGuid().ToString();
    static readonly string? IsPreviouslyEngagedWithNetwork = "true";
    static readonly string PreviousEngagementUrl = Guid.NewGuid().ToString();
    static readonly string LocallyPreferredRegion = Guid.NewGuid().ToString();
    static readonly List<RegionModel> LocalOrganisationMultipleRegions = new()
        {
            new  RegionModel() { Area = LocallyPreferredRegion, IsSelected = true, IsConfirmed = true},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false}
        };

    static readonly List<RegionModel> MultiOrganisationRegions = new()
        {
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = true, IsConfirmed = false}
        };

    static readonly string OnlySelectedRegion = Guid.NewGuid().ToString();
    static readonly List<RegionModel> SingleRegionSelected = new()
        {
            new  RegionModel() { Area = OnlySelectedRegion, IsSelected = true, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = false, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = false, IsConfirmed = false},
            new  RegionModel() { Area = Guid.NewGuid().ToString(), IsSelected = false, IsConfirmed = false}
        };

    static readonly List<ProfileModel> ProfileValues = new List<ProfileModel>()
        {
            new ProfileModel { Id = 1, Category = Category.ReasonToJoin, Value = Guid.NewGuid().ToString() },
            new ProfileModel { Id = 2, Category = Category.ReasonToJoin, Value = Guid.NewGuid().ToString() },
            new ProfileModel { Id = 3, Category = Category.ReasonToJoin, Value = null },
            new ProfileModel { Id = 4, Category = Category.Support, Value = Guid.NewGuid().ToString() },
            new ProfileModel { Id = 5, Category = Category.Support, Value = Guid.NewGuid().ToString() },
            new ProfileModel { Id = 6, Category = Category.Support, Value = null },
            new ProfileModel { Id = ProfileDataId.HasPreviousEngagement, Value = IsPreviouslyEngagedWithNetwork }
        };


    [SetUp]
    public void Init()
    {
        _employerAccountId = Guid.NewGuid().ToString();
        _sessionModel = new();
        _sessionServiceMock = new();
        _sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(_sessionModel);
        _outerApiClientMock = new();
        _validatorMock = new();
        _sut = new(_sessionServiceMock.Object, _outerApiClientMock.Object, _validatorMock.Object);

        _sut.AddUrlHelperMock()
        .AddUrlForRoute(RouteNames.Onboarding.Regions, RegionUrl)
        .AddUrlForRoute(RouteNames.Onboarding.JoinTheNetwork, ReasonToJoinTheNetworkUrl)
        .AddUrlForRoute(RouteNames.Onboarding.PreviousEngagement, PreviousEngagementUrl);

        user = UsersForTesting.GetUserWithClaims(_employerAccountId);
        _sessionModel.EmployerDetails.FullName = user.GetIdamsUserDisplayName();
        _sessionModel.EmployerDetails.Email = user.GetEmail();
        var account = user.GetEmployerAccount(_employerAccountId);
        _sessionModel.EmployerDetails.OrganisationName = account.DasAccountName;

        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        _sessionModel.Regions = LocalOrganisationMultipleRegions;
        _sessionModel.ProfileData = ProfileValues;
    }

    private void InvokeControllerGet()
    {
        _getResult = _sut.Get(_employerAccountId).As<ViewResult>();
        _viewModel = _getResult.Model.As<CheckYourAnswersViewModel>();
    }

    [Test]
    public void ThenReturnsViewResults()
    {
        InvokeControllerGet();
        _getResult.Should().NotBeNull();
        _getResult!.ViewName.Should().Be(CheckYourAnswersController.ViewPath);
    }

    [Test]
    public void ThenSetsRegionChangeLinkInViewModel()
    {
        InvokeControllerGet();
        _viewModel!.RegionChangeLink.Should().Be(RegionUrl);
    }

    [Test]
    public void ThenSetsSingleRegionInViewModel()
    {
        _sessionModel.Regions = SingleRegionSelected;
        InvokeControllerGet();
        _viewModel!.Region.Should().Equal(OnlySelectedRegion);
    }

    [Test]
    public void ThenSetsLocallyPreferredRegionInViewModel()
    {
        _sessionModel.Regions = LocalOrganisationMultipleRegions;
        InvokeControllerGet();
        var result = LocalOrganisationMultipleRegions.Where(x => x.IsSelected).Select(x => x.Area).ToList();
        result.Add($"Locally prefers {LocallyPreferredRegion}");

        _viewModel!.Region.Should().Equal(result);
    }

    [Test]
    public void ThenSetsMultiRegionalOrganisationInViewModel()
    {
        _sessionModel.Regions = MultiOrganisationRegions;
        _sessionModel.IsLocalOrganisation = false;
        InvokeControllerGet();
        var result = MultiOrganisationRegions.Where(x => x.IsSelected).Select(x => x.Area).ToList();
        result.Add($"Prefers to engage as multi-regional");

        _viewModel!.Region.Should().Equal(result);
    }

    [Test]
    public void ThenSetsReasonToJoinAndSupportNeededInViewModel()
    {
        InvokeControllerGet();
        _viewModel!.ReasonChangeLink.Should().Be(ReasonToJoinTheNetworkUrl);
        _viewModel.Reason.Should().Equal(ProfileValues.Where(x => (x.Category == Category.ReasonToJoin) && x.Value != null).Select(x => x.Description).ToList());
        _viewModel.Support.Should().Equal(ProfileValues.Where(x => (x.Category == Category.Support) && x.Value != null).Select(x => x.Description).ToList());
    }

    [TestCase("true")]
    [TestCase("false")]
    [TestCase(null)]
    public void ThenSetsPreviousEngagementInViewModel(string isPreviouslyEngagged)
    {
        _sessionModel.SetProfileValue(ProfileDataId.HasPreviousEngagement, isPreviouslyEngagged);
        _getResult = _sut.Get(_employerAccountId).As<ViewResult>();
        _viewModel = _getResult.Model.As<CheckYourAnswersViewModel>();

        _viewModel.PreviousEngagement.Should().Be(CheckYourAnswersViewModel.GetPreviousEngagementValue(isPreviouslyEngagged));
        _viewModel.PreviousEngagementChangeLink.Should().Be(PreviousEngagementUrl);
    }

    [Test]
    public void ThenSetsEmployersDetailsInViewModel()
    {
        InvokeControllerGet();
        _viewModel!.FullName.Should().Be(_sessionModel.EmployerDetails.FullName);
        _viewModel!.Email.Should().Be(_sessionModel.EmployerDetails.Email);
        _viewModel!.OrganisationName.Should().Be(_sessionModel.EmployerDetails.OrganisationName);
        _viewModel!.ActiveApprenticesCount.Should().Be(_sessionModel.EmployerDetails.ActiveApprenticesCount);
        _viewModel!.DigitalApprenticeshipProgrammeStartDate.Should().Be(_sessionModel.EmployerDetails.DigitalApprenticeshipProgrammeStartDate);
        _viewModel!.Sectors.Should().BeEquivalentTo(_sessionModel.EmployerDetails.Sectors);
    }

    [TearDown]
    public void Dispose()
    {
        _sessionModel = null!;
        _sut = null!;
        _getResult = null!;
        _viewModel = null!;
        _sessionServiceMock = null!;
    }
}
