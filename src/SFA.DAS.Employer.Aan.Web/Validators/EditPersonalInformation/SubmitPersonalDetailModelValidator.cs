﻿using FluentValidation;
using SFA.DAS.Aan.SharedUi.Models;

namespace SFA.DAS.Employer.Aan.Web.Validators.EditPersonalInformation;

public class SubmitPersonalDetailModelValidator : AbstractValidator<SubmitPersonalDetailModel>
{
    public const string BiographyValidationMessage = "Your biography must be 500 characters or less";
    public const string JobTitleValidationMessage = "Your job title must be 200 characters or less";
    public const string JobTitlePatternValidationMessage = "Your job title must be alphanumeric";
    public const string BiographyHasExcludedCharacter = "Your biography must not include any special characters: @, #, $, ^, =, +, \\, /, <, >, %";
    public const string ExcludedCharactersRegex = @"^[^@#$^=+\\\/<>%]*$";

    public const string JobTitlePatternRegex = "^[a-zA-Z0-9 ]*$";
    public SubmitPersonalDetailModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Biography).Length(0, 500)
            .WithMessage(BiographyValidationMessage)
            .Matches(ExcludedCharactersRegex)
            .WithMessage(BiographyHasExcludedCharacter)
            ;
        RuleFor(x => x.JobTitle).Matches(JobTitlePatternRegex)
            .WithMessage(JobTitlePatternValidationMessage).Length(0, 200).WithMessage(JobTitleValidationMessage);
    }
}
