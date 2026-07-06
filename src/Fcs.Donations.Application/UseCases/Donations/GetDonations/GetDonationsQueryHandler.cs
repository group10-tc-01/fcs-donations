using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.Common.Pagination;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.Messages;

namespace Fcs.Donations.Application.UseCases.Donations.GetDonations;

public sealed class GetDonationsQueryHandler : IQueryHandler<GetDonationsQuery, PagedResponse<DonationQueryResponse>>
{
    private readonly IDonationRepository _donationRepository;
    private readonly ICurrentUser _currentUser;

    public GetDonationsQueryHandler(
        IDonationRepository donationRepository,
        ICurrentUser currentUser)
    {
        _donationRepository = donationRepository;
        _currentUser = currentUser;
    }

    public Task<Result<PagedResponse<DonationQueryResponse>>> Handle(
        GetDonationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Task.FromResult<Result<PagedResponse<DonationQueryResponse>>>(
                Error.Failure(
                    ResourceMessages.DonationUnauthenticatedCode,
                    ResourceMessages.DonationUnauthenticated));
        }

        var query = _donationRepository.Query();

        if (!_currentUser.Roles.Contains("GestorONG"))
        {
            if (!Guid.TryParse(_currentUser.KeycloakUserId, out var donorId))
            {
                return Task.FromResult<Result<PagedResponse<DonationQueryResponse>>>(
                    Error.Failure(
                        ResourceMessages.DonationUnauthenticatedCode,
                        ResourceMessages.DonationUnauthenticated));
            }

            query = query.Where(donation => donation.DonorId == donorId);
        }

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
