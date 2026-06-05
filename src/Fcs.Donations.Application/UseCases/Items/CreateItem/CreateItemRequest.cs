using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.UseCases.Items.CreateItem;

public sealed record CreateItemRequest(string Name, decimal Price) : ICommand<CreateItemResponse>;
