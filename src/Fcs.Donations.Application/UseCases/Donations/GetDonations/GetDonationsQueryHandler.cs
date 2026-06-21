using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.Messages;

namespace Fcs.Donations.Application.UseCases.Donations.GetDonations;

public sealed class GetDonationsQueryHandler : IQueryHandler<GetDonationsQuery, IQueryable<DonationQueryResponse>>
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

    public Task<Result<IQueryable<DonationQueryResponse>>> Handle(
        GetDonationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !Guid.TryParse(_currentUser.KeycloakUserId, out var donorId))
        {
            return Task.FromResult<Result<IQueryable<DonationQueryResponse>>>(
                Error.Failure(
                    ResourceMessages.DonationUnauthenticatedCode,
                    ResourceMessages.DonationUnauthenticated));
        }

        var donations = _donationRepository.Query()
            .Where(donation => donation.DonorId == donorId)
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
            });

        var result = Result<IQueryable<DonationQueryResponse>>.Success(donations);
        return Task.FromResult(result);
    }
}
