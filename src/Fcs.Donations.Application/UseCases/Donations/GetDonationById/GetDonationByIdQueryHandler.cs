using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.Messages;

namespace Fcs.Donations.Application.UseCases.Donations.GetDonationById;

public sealed class GetDonationByIdQueryHandler : IQueryHandler<GetDonationByIdQuery, DonationQueryResponse>
{
    private readonly IDonationRepository _donationRepository;
    private readonly ICurrentUser _currentUser;

    public GetDonationByIdQueryHandler(
        IDonationRepository donationRepository,
        ICurrentUser currentUser)
    {
        _donationRepository = donationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<DonationQueryResponse>> Handle(
        GetDonationByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Error.Failure(
                ResourceMessages.DonationUnauthenticatedCode,
                ResourceMessages.DonationUnauthenticated);
        }

        var donation = await _donationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (donation is null)
        {
            return DonationNotFound();
        }

        if (!_currentUser.Roles.Contains("GestorONG"))
        {
            if (!Guid.TryParse(_currentUser.KeycloakUserId, out var donorId))
            {
                return Error.Failure(
                    ResourceMessages.DonationUnauthenticatedCode,
                    ResourceMessages.DonationUnauthenticated);
            }

            if (donation.DonorId != donorId)
            {
                return DonationNotFound();
            }
        }

        return DonationQueryResponse.FromDomain(donation);
    }

    private static Error DonationNotFound() =>
        Error.NotFound(
            ResourceMessages.DonationNotFoundCode,
            ResourceMessages.DonationNotFound);
}
