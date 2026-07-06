using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;

namespace Fcs.Donations.Application.UseCases.Donations.GetDonationById;

public sealed record GetDonationByIdQuery(Guid Id) : IQuery<DonationQueryResponse>;
