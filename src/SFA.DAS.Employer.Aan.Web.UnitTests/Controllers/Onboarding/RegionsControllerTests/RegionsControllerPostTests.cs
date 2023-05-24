using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using SFA.DAS.Employer.Aan.Domain.Interfaces;
using SFA.DAS.Employer.Aan.Web.Controllers.Onboarding;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;
using SFA.DAS.Employer.Aan.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.Aan.Web.UnitTests.Controllers.Onboarding.RegionsControllerTests;

[TestFixture]
public class RegionsControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Post_SetsSelectedRegionsInOnBoardingSessionModel(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RegionsSubmitModel>> validatorMock,
        [Greedy] RegionsController sut,
        RegionsSubmitModel submitmodel,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock();
        OnboardingSessionModel sessionModel = new();
        sessionServiceMock.Setup(s => s.Get<OnboardingSessionModel>()).Returns(sessionModel);

        ValidationResult validationResult = new();
        validatorMock.Setup(v => v.Validate(submitmodel)).Returns(validationResult);

        await sut.Post(submitmodel, cancellationToken);
        sessionServiceMock.Verify(s => s.Set(It.Is<OnboardingSessionModel>(m => m.Regions == submitmodel.Regions)));
    }

    [Test, MoqAutoData]
    public async Task Post_Errors_WhenNoSelectedRegions(
        [Greedy] RegionsController sut,
        CancellationToken cancellationToken)
    {
        sut.AddUrlHelperMock();
        RegionsSubmitModel submitmodel = new()
        {
            Regions = null!
        };

        await sut.Post(submitmodel, cancellationToken);
        sut.ModelState.IsValid.Should().BeFalse();
    }
}
