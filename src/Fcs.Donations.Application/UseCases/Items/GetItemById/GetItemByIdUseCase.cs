using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Domain;
using Fcs.Donations.Domain.Items;
using Fcs.Donations.Messages;

namespace Fcs.Donations.Application.UseCases.Items.GetItemById;

public sealed class GetItemByIdUseCase : IGetItemByIdUseCase
{
    private readonly IItemRepository _itemRepository;

    public GetItemByIdUseCase(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<Result<GetItemByIdResponse>> Handle(GetItemByIdRequest request, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(request.Id, cancellationToken);

        if (item is null)
        {
            return Error.NotFound(ResourceMessages.ItemNotFoundCode, ResourceMessages.ItemNotFound);
        }

        return new GetItemByIdResponse(item.Id, item.Name, item.Price, item.CreatedAt);
    }
}
