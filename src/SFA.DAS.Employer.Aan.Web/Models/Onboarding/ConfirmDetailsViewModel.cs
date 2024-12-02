namespace SFA.DAS.Employer.Aan.Web.Models.Onboarding
{
    public class ConfirmDetailsViewModel : ConfirmDetailsSubmitModel, IBackLink
    {
        public string BackLink { get; set; } = null!;
        public string FullName { get; set; } = "";
        public string EmailAddress { get; set; } = "";
        public string EmployerName { get; set; } = "";

        public int ActiveApprenticesCount { get; set; }
        public List<string> Sectors { get; set; } = [];
    }

    public class ConfirmDetailsSubmitModel : ViewModelBase
    {
    }
}
