using Fcg.Donations.Application.Abstractions.Messaging;
using Fcg.Donations.Domain;
using Fcg.Donations.Domain.Items;
using Fcg.Donations.Messages;

namespace Fcg.Donations.Application.UseCases.Items.GetItemById;

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
            return Error.NotFound("Item.NotFound", ResourceMessages.ItemNotFound);
        }

        return new GetItemByIdResponse(item.Id, item.Name, item.Price, item.CreatedAt);
    }
}
