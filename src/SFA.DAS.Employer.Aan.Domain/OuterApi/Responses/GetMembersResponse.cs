﻿using SFA.DAS.Aan.SharedUi.OuterApi.Responses;

namespace SFA.DAS.Employer.Aan.Domain.OuterApi.Responses;
public class GetMembersResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<MemberSummary> Members { get; set; } = Enumerable.Empty<MemberSummary>();
}
