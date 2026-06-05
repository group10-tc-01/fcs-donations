using fcs.Donations.Application.Abstractions.Messaging;

namespace fcs.Donations.Application.UseCases.Items.CreateItem;

public sealed record CreateItemRequest(string Name, decimal Price) : ICommand<CreateItemResponse>;
