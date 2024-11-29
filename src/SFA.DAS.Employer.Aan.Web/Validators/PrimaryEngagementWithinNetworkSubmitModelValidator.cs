using FluentValidation;
using SFA.DAS.Employer.Aan.Web.Models.Onboarding;

namespace SFA.DAS.Employer.Aan.Web.Validators;

public class PrimaryEngagementWithinNetworkSubmitModelValidator : AbstractValidator<PrimaryEngagementWithinNetworkSubmitModel>
{
    public const string NoSelectionErrorMessage = "Select which network you want to join";

    public PrimaryEngagementWithinNetworkSubmitModelValidator()
    {
        RuleFor(m => m.IsMultiRegionalOrganisation)
            .NotNull()
            .WithMessage(NoSelectionErrorMessage);
    }
}