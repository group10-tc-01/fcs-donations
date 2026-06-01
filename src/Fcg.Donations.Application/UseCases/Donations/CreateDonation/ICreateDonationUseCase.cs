using Fcg.Donations.Application.Abstractions.Messaging;

namespace Fcg.Donations.Application.UseCases.Donations.CreateDonation;

public interface ICreateDonationUseCase : ICommandHandler<CreateDonationRequest, CreateDonationResponse>
{
}
