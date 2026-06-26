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
            .Select(donation => new DonationQueryResponse
            {
                Id = donation.Id,
                CampaignId = donation.CampaignId,
                DonorId = donation.DonorId,
                Amount = donation.Amount,
                Status = donation.Status,
                CreatedAt = donation.CreatedAt,
                ProcessedAt = donation.ProcessedAt,
                FailureReason = donation.FailureReason
            })
            .ToList();

        var page = PagedResponse<DonationQueryResponse>.NormalizePage(request.Page);
        var pageSize = PagedResponse<DonationQueryResponse>.NormalizePageSize(request.PageSize);

        var sorted = (request.SortBy?.ToLowerInvariant(), request.SortDescending) switch
        {
            ("amount", false) => donations.OrderBy(x => x.Amount),
            ("amount", true) => donations.OrderByDescending(x => x.Amount),
            ("status", false) => donations.OrderBy(x => x.Status),
            ("status", true) => donations.OrderByDescending(x => x.Status),
            ("createdat", false) => donations.OrderBy(x => x.CreatedAt),
            ("createdat", true) => donations.OrderByDescending(x => x.CreatedAt),
            (_, false) => donations.OrderBy(x => x.CreatedAt),
            (_, true) => donations.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = Result<PagedResponse<DonationQueryResponse>>.Success(
            new PagedResponse<DonationQueryResponse>(items, page, pageSize, totalCount));

        return Task.FromResult(result);
    }
}
