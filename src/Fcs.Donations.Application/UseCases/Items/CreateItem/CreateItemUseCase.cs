using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Domain;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Items;
using Fcs.Donations.Messages;

namespace Fcs.Donations.Application.UseCases.Items.CreateItem;

public sealed class CreateItemUseCase : ICreateItemUseCase
{
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;

    public CreateItemUseCase(IItemRepository itemRepository, IUnitOfWork unitOfWork, IMessagePublisher messagePublisher)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result<CreateItemResponse>> Handle(CreateItemRequest request, CancellationToken cancellationToken)
    {
        if (await _itemRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            return Error.Conflict(ResourceMessages.ItemAlreadyExistsCode, ResourceMessages.ItemAlreadyExists);
        }

        var itemResult = Item.Create(request.Name, request.Price);

        if (itemResult.IsFailure)
        {
            return itemResult.Error;
        }

        var item = itemResult.Value;

        await _itemRepository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _messagePublisher.PublishAsync(
            new ItemCreatedMessage(item.Id, item.Name, item.Price, item.CreatedAt),
            cancellationToken);

        return new CreateItemResponse(item.Id, item.Name, item.Price, item.CreatedAt);
    }
}
