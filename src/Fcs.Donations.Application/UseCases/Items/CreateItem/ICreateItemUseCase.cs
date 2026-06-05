using fcs.Donations.Application.Abstractions.Messaging;

namespace fcs.Donations.Application.UseCases.Items.CreateItem;

public interface ICreateItemUseCase : ICommandHandler<CreateItemRequest, CreateItemResponse>
{
}
