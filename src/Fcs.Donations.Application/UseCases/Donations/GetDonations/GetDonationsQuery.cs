using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.UseCases.Donations.GetDonations;

public sealed record GetDonationsQuery : IQuery<IQueryable<DonationQueryResponse>>;
