using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.UseCases.Donations.CreateDonation;

public interface ICreateDonationUseCase : ICommandHandler<CreateDonationRequest, CreateDonationResponse>
{
}
