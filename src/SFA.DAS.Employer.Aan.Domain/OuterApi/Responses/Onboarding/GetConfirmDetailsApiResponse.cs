namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses.Onboarding
{
    public class GetConfirmDetailsApiResponse
    {
        public int NumberOfActiveApprentices { get; set; }
        public List<string> Sectors { get; set; } = [];
    }
}
