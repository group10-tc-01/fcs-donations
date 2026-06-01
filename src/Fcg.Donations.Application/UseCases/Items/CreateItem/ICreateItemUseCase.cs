using Fcg.Donations.Application.Abstractions.Messaging;

namespace Fcg.Donations.Application.UseCases.Items.CreateItem;

public interface ICreateItemUseCase : ICommandHandler<CreateItemRequest, CreateItemResponse>
{
}
