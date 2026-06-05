using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.UseCases.Items.CreateItem;

public interface ICreateItemUseCase : ICommandHandler<CreateItemRequest, CreateItemResponse>
{
}
