using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.Common.Pagination;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Results;

namespace Fcs.Donations.Application.UseCases.Donations.GetAdminDonations;

public sealed class GetAdminDonationsQueryHandler : IQueryHandler<GetAdminDonationsQuery, PagedResponse<DonationQueryResponse>>
{
    private readonly IDonationRepository _donationRepository;

    public GetAdminDonationsQueryHandler(IDonationRepository donationRepository)
    {
        _donationRepository = donationRepository;
    }

    public Task<Result<PagedResponse<DonationQueryResponse>>> Handle(
        GetAdminDonationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _donationRepository.Query();

        if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == request.Status.Value);
        }

        var donations = query
            .Select(DonationQueryResponse.FromDomain)
            .ToList();

        var paged = DonationSortHelper.ApplyPagination(
            donations, request.Page, request.PageSize, request.SortBy, request.SortDescending);

        var result = Result<PagedResponse<DonationQueryResponse>>.Success(paged);

        return Task.FromResult(result);
    }
}
