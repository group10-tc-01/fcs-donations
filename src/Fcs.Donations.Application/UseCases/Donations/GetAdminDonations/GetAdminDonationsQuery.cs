using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.Common.Pagination;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.Domain.Donations;

namespace Fcs.Donations.Application.UseCases.Donations.GetAdminDonations;

public sealed record GetAdminDonationsQuery(
    int Page = 1,
    int PageSize = 10,
    DonationStatus? Status = null,
    string? SortBy = null,
    bool SortDescending = false) : IQuery<PagedResponse<DonationQueryResponse>>;
