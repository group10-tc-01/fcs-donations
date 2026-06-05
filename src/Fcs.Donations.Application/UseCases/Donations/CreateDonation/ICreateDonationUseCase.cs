using fcs.Donations.Application.Abstractions.Messaging;

namespace fcs.Donations.Application.UseCases.Donations.CreateDonation;

public interface ICreateDonationUseCase : ICommandHandler<CreateDonationRequest, CreateDonationResponse>
{
}
